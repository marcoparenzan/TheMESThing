using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using TheMESThing.Contracts;
using TheMESThingData;
using TheMESThingAPI.Python;
using TheMESThingData.Entities.Analytics;
using TheMESThingData.Entities.Iot;
using PySharpLib.Runtime;

namespace TheMESThingAPI.Endpoints;

/// <summary>
/// Typed telemetry ingestion: request bodies are Ontly-generated contract types, so units and
/// value constraints are validated at deserialization. Readings are stored in the same
/// MachineTelemetry table as the generic endpoint, always in the quantity's canonical unit.
/// Wire format: a quantity is a [value, "canonical unit symbol"] pair, e.g. [293.15, "K"].
/// </summary>
public static class TypedTelemetryEndpoints
{
    public static IEndpointRouteBuilder MapTypedTelemetryEndpoints(this IEndpointRouteBuilder app)
    {
        var g = app.MapGroup("/api/iot/telemetry/readings").WithTags("IoT – Typed telemetry readings");

        g.MapPost("/temperature", (MachineTemperatureReading r, TheMESThingDbContext db) =>
            Ingest(db, r.MachineId, "Temperature", r.Value.Value, TemperatureMeasure<double>.CanonicalUnitSymbol, r.RecordedAt))
            .WithName("IngestTemperatureReading").WithSummary("Ingest a temperature reading (canonical unit K)");

        g.MapPost("/line-speed", (MachineLineSpeedReading r, TheMESThingDbContext db) =>
            Ingest(db, r.MachineId, "LineSpeed", r.Value.Value, LineSpeedMeasure<double>.CanonicalUnitSymbol, r.RecordedAt))
            .WithName("IngestLineSpeedReading").WithSummary("Ingest a line speed reading (canonical unit m/min)");

        g.MapPost("/vibration", (MachineVibrationReading r, TheMESThingDbContext db) =>
            Ingest(db, r.MachineId, "Vibration", r.Value.Value, VibrationMeasure<double>.CanonicalUnitSymbol, r.RecordedAt))
            .WithName("IngestVibrationReading").WithSummary("Ingest a vibration reading (canonical unit m/s²)");

        g.MapPost("/cycle-time", (MachineCycleTimeReading r, TheMESThingDbContext db) =>
            Ingest(db, r.MachineId, "CycleTime", r.Value.Value, CycleTimeMeasure<double>.CanonicalUnitSymbol, r.RecordedAt))
            .WithName("IngestCycleTimeReading").WithSummary("Ingest a cycle time reading (canonical unit s)");

        // Status reported by the device (Running / Idle / Down / Offline): updates the current status of the machine.
        app.MapPost("/api/iot/machines/status", async (MachineStatusReport r, TheMESThingDbContext db) =>
        {
            var machine = await db.Machines.FirstOrDefaultAsync(m => m.MachineId == r.MachineId.Value);
            if (machine is null) return Results.NotFound();

            var status = r.Status.Value;
            var at = r.ReportedAt.Value.ToUniversalTime();
            if (!string.Equals(machine.CurrentStatus, status, StringComparison.Ordinal))
            {
                machine.CurrentStatus = status;
                machine.LastStatusChangedAtUtc = at;
            }
            machine.IoTHubConnectionState = status == "Offline" ? "Disconnected" : "Connected";
            machine.UpdatedAtUtc = DateTime.UtcNow;
            await db.SaveChangesAsync();
            return Results.NoContent();
        })
        .WithTags("IoT – Typed telemetry readings")
        .WithName("ReportMachineStatus").WithSummary("Report the status of a machine (Running, Idle, Down, Offline)");

        // Live status: the reported status combined with whether the machine is actually sending telemetry.
        app.MapGet("/api/iot/machines/live-status", async (TheMESThingDbContext db, int onlineWithinSeconds = 30) =>
            Results.Ok(await LiveStatusAsync(db, onlineWithinSeconds)))
        .WithTags("IoT – Typed telemetry readings")
        .WithName("GetMachinesLiveStatus")
        .WithSummary("Status of every machine: reported status, last telemetry time, online flag, and the effective status (Offline when no recent telemetry)");

        // Production counters: the inputs of the OEE calculation. The API runs the OEE rule (Python plugin
        // oee.py) on them and stores the counters together with the computed availability/performance/quality/OEE.
        g.MapPost("/counters", async (MachineProductionCounters c, TheMESThingDbContext db, PythonPluginHost plugins) =>
        {
            var machine = await db.Machines.AsNoTracking().FirstOrDefaultAsync(m => m.MachineId == c.MachineId.Value);
            if (machine is null) return Results.NotFound();

            long planned = ToSeconds(c.PlannedProductionTime), operating = ToSeconds(c.OperatingTime), run = ToSeconds(c.RunTime);
            var totalQuantity = c.TotalQuantity.Value;
            var goodQuantity = c.GoodQuantity.Value;

            Dictionary<string, object?> result;
            try
            {
                result = (Dictionary<string, object?>)plugins.Invoke("oee", "calculate", planned, operating, run, goodQuantity, totalQuantity)!;
            }
            catch (PyRaise ex)
            {
                return Results.BadRequest(new { error = ex.Message });
            }

            var periodEnd = c.PeriodEnd.Value.ToUniversalTime();
            var entity = new OeeCalculation
            {
                TenantId = machine.TenantId,
                MachineId = machine.MachineId,
                PeriodDate = DateOnly.FromDateTime(periodEnd),
                PeriodType = c.PeriodType.Value,
                PlannedProductionTimeSeconds = planned,
                OperatingTimeSeconds = operating,
                RunTimeSeconds = run,
                PlannedQuantity = (decimal)c.PlannedQuantity.Value,
                GoodQuantity = (decimal)goodQuantity,
                TotalQuantity = (decimal)totalQuantity,
                Availability = Ratio(result, "availability"),
                Performance = Ratio(result, "performance"),
                Quality = Ratio(result, "quality"),
                OeeValue = Ratio(result, "oee"),
                UnscheduledDowntimeSeconds = planned - operating,
                CalculatedAtUtc = periodEnd,
            };
            db.OeeCalculations.Add(entity);
            await db.SaveChangesAsync();
            return Results.Created($"/api/analytics/oee/{entity.OeeCalculationId}", entity);
        })
        .WithName("IngestProductionCounters").WithSummary("Ingest production counters (planned/operating/run time, total/good quantity, reference period); OEE is computed by the Python plugin");

        return app;
    }

    public sealed record MachineLiveStatus(
        Guid MachineId, string MachineCode, string MachineName,
        string ReportedStatus, string? ConnectionState, DateTime? LastStatusChangedAtUtc,
        DateTime? LastTelemetryAtUtc, bool IsOnline, string EffectiveStatus);

    /// <summary>A machine is online when it sent telemetry within the last <paramref name="onlineWithinSeconds"/>;
    /// otherwise its effective status is "Offline", whatever status was last stored.</summary>
    public static async Task<List<MachineLiveStatus>> LiveStatusAsync(TheMESThingDbContext db, int onlineWithinSeconds = 30)
    {
        var since = DateTime.UtcNow.AddSeconds(-Math.Max(1, onlineWithinSeconds));
        var lastSeen = await db.MachineTelemetries.AsNoTracking()
            .Where(t => t.RecordedAtUtc >= since)
            .GroupBy(t => t.MachineId)
            .Select(g => new { MachineId = g.Key, Last = g.Max(x => x.RecordedAtUtc) })
            .ToDictionaryAsync(x => x.MachineId, x => x.Last);

        var machines = await db.Machines.AsNoTracking().OrderBy(m => m.MachineCode)
            .Select(m => new { m.MachineId, m.MachineCode, m.MachineName, m.CurrentStatus, m.IoTHubConnectionState, m.LastStatusChangedAtUtc })
            .ToListAsync();

        return machines.Select(m =>
        {
            var online = lastSeen.TryGetValue(m.MachineId, out var last);
            return new MachineLiveStatus(m.MachineId, m.MachineCode, m.MachineName, m.CurrentStatus, m.IoTHubConnectionState,
                m.LastStatusChangedAtUtc, online ? last : null, online, online ? m.CurrentStatus : "Offline");
        }).ToList();
    }

    // Durations are stored as canonical seconds.
    static long ToSeconds(DurationMeasure<double> duration) => (long)Math.Round(duration.Value);

    // The plugin returns a ratio (0..1) or null when it cannot be computed.
    static decimal? Ratio(Dictionary<string, object?> result, string key) =>
        result.TryGetValue(key, out var v) && v is not null ? Convert.ToDecimal(v, System.Globalization.CultureInfo.InvariantCulture) : null;

    static async Task<Results<Created<MachineTelemetry>, NotFound>> Ingest(
        TheMESThingDbContext db, MachineId machineId, string metricName, double value, string unit, EventTimestamp recordedAt)
    {
        var machine = await db.Machines.AsNoTracking().FirstOrDefaultAsync(m => m.MachineId == machineId.Value);
        if (machine is null) return TypedResults.NotFound();

        var recordedAtUtc = recordedAt.Value.ToUniversalTime();
        var entity = new MachineTelemetry
        {
            TenantId = machine.TenantId,
            MachineId = machine.MachineId,
            MetricName = metricName,
            MetricValue = value,
            MetricUnit = unit,
            RecordedAtUtc = recordedAtUtc,
            TelemetryDateUtc = DateOnly.FromDateTime(recordedAtUtc)
        };
        db.MachineTelemetries.Add(entity);
        await db.SaveChangesAsync();
        return TypedResults.Created($"/api/iot/telemetry/{entity.MachineTelemetryId}", entity);
    }
}
