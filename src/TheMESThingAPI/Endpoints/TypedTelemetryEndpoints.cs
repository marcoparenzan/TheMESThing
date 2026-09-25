using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using TheMESThing.Contracts;
using TheMESThingData;
using TheMESThingData.Entities.Iot;

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

        return app;
    }

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
