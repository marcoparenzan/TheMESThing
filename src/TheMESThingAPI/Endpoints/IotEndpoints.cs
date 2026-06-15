using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using TheMESThingAPI.Common;
using TheMESThingData;
using TheMESThingData.Entities.Iot;

namespace TheMESThingAPI.Endpoints;

public static class IotEndpoints
{
    public static IEndpointRouteBuilder MapIotEndpoints(this IEndpointRouteBuilder app)
    {
        MapMachineStates(app);
        MapMachineStatusHistories(app);
        MapMachineTelemetries(app);
        MapMachineEvents(app);
        MapMachineCycles(app);
        return app;
    }

    // ── MachineStates ─────────────────────────────────────────────────────────
    static void MapMachineStates(IEndpointRouteBuilder app)
    {
        var g = app.MapGroup("/api/iot/machine-states").WithTags("IoT – MachineStates");

        g.MapGet("/", async (TheMESThingDbContext db) =>
            await db.MachineStates.AsNoTracking().OrderBy(s => s.SortOrder).ToListAsync())
        .WithName("GetMachineStates").WithSummary("List all machine states");

        g.MapGet("/{id:int}", async Task<Results<Ok<MachineState>, NotFound>> (int id, TheMESThingDbContext db) =>
        {
            var e = await db.MachineStates.AsNoTracking().FirstOrDefaultAsync(s => s.MachineStateId == id);
            return e is null ? TypedResults.NotFound() : TypedResults.Ok(e);
        })
        .WithName("GetMachineState").WithSummary("Get a machine state by ID");

        g.MapPost("/", async Task<Created<MachineState>> (MachineState body, TheMESThingDbContext db) =>
        {
            db.MachineStates.Add(body);
            await db.SaveChangesAsync();
            return TypedResults.Created($"/api/iot/machine-states/{body.MachineStateId}", body);
        })
        .WithName("CreateMachineState").WithSummary("Create a machine state");

        g.MapPut("/{id:int}", async Task<Results<Ok<MachineState>, NotFound>> (int id, MachineState body, TheMESThingDbContext db) =>
        {
            var e = await db.MachineStates.FindAsync(id);
            if (e is null) return TypedResults.NotFound();
            e.StateCode = body.StateCode;
            e.StateName = body.StateName;
            e.StateCategory = body.StateCategory;
            e.IsProductiveTime = body.IsProductiveTime;
            e.IsPlannedStop = body.IsPlannedStop;
            e.ColorHex = body.ColorHex;
            e.SortOrder = body.SortOrder;
            await db.SaveChangesAsync();
            return TypedResults.Ok(e);
        })
        .WithName("UpdateMachineState").WithSummary("Update a machine state");

        g.MapDelete("/{id:int}", async Task<Results<NoContent, NotFound>> (int id, TheMESThingDbContext db) =>
        {
            var e = await db.MachineStates.FindAsync(id);
            if (e is null) return TypedResults.NotFound();
            db.MachineStates.Remove(e);
            await db.SaveChangesAsync();
            return TypedResults.NoContent();
        })
        .WithName("DeleteMachineState").WithSummary("Delete a machine state");
    }

    // ── MachineStatusHistories ────────────────────────────────────────────────
    static void MapMachineStatusHistories(IEndpointRouteBuilder app)
    {
        var g = app.MapGroup("/api/iot/machine-status-history").WithTags("IoT – MachineStatusHistory");

        g.MapGet("/", async (TheMESThingDbContext db, Guid? machineId, DateOnly? from, DateOnly? to, int page = 1, int pageSize = 50) =>
        {
            var q = db.MachineStatusHistories.AsNoTracking();
            if (machineId.HasValue) q = q.Where(h => h.MachineId == machineId.Value);
            if (from.HasValue) q = q.Where(h => h.EventDateUtc >= from.Value);
            if (to.HasValue) q = q.Where(h => h.EventDateUtc <= to.Value);
            var total = await q.CountAsync();
            var items = await q.OrderByDescending(h => h.StartedAtUtc)
                .Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();
            return new PagedResult<MachineStatusHistory>(items, page, pageSize, total);
        })
        .WithName("GetMachineStatusHistories").WithSummary("List machine status history");

        g.MapGet("/{id:long}", async Task<Results<Ok<MachineStatusHistory>, NotFound>> (long id, TheMESThingDbContext db) =>
        {
            var e = await db.MachineStatusHistories.AsNoTracking()
                .FirstOrDefaultAsync(h => h.MachineStatusHistoryId == id);
            return e is null ? TypedResults.NotFound() : TypedResults.Ok(e);
        })
        .WithName("GetMachineStatusHistory").WithSummary("Get a machine status history entry");

        g.MapPost("/", async Task<Created<MachineStatusHistory>> (MachineStatusHistory body, TheMESThingDbContext db) =>
        {
            db.MachineStatusHistories.Add(body);
            await db.SaveChangesAsync();
            return TypedResults.Created($"/api/iot/machine-status-history/{body.MachineStatusHistoryId}", body);
        })
        .WithName("CreateMachineStatusHistory").WithSummary("Create a machine status history entry");

        g.MapPut("/{id:long}", async Task<Results<Ok<MachineStatusHistory>, NotFound>> (long id, MachineStatusHistory body, TheMESThingDbContext db) =>
        {
            var e = await db.MachineStatusHistories
                .FirstOrDefaultAsync(h => h.MachineStatusHistoryId == id);
            if (e is null) return TypedResults.NotFound();
            e.EndedAtUtc = body.EndedAtUtc;
            e.ReasonCode = body.ReasonCode;
            e.Notes = body.Notes;
            await db.SaveChangesAsync();
            return TypedResults.Ok(e);
        })
        .WithName("UpdateMachineStatusHistory").WithSummary("Update a machine status history entry");

        g.MapDelete("/{id:long}", async Task<Results<NoContent, NotFound>> (long id, TheMESThingDbContext db) =>
        {
            var e = await db.MachineStatusHistories
                .FirstOrDefaultAsync(h => h.MachineStatusHistoryId == id);
            if (e is null) return TypedResults.NotFound();
            db.MachineStatusHistories.Remove(e);
            await db.SaveChangesAsync();
            return TypedResults.NoContent();
        })
        .WithName("DeleteMachineStatusHistory").WithSummary("Delete a machine status history entry");
    }

    // ── MachineTelemetries ────────────────────────────────────────────────────
    static void MapMachineTelemetries(IEndpointRouteBuilder app)
    {
        var g = app.MapGroup("/api/iot/telemetry").WithTags("IoT – MachineTelemetry");

        g.MapGet("/", async (TheMESThingDbContext db, Guid? machineId, string? metricName, DateOnly? from, DateOnly? to, int page = 1, int pageSize = 100) =>
        {
            var q = db.MachineTelemetries.AsNoTracking();
            if (machineId.HasValue) q = q.Where(t => t.MachineId == machineId.Value);
            if (!string.IsNullOrEmpty(metricName)) q = q.Where(t => t.MetricName == metricName);
            if (from.HasValue) q = q.Where(t => t.TelemetryDateUtc >= from.Value);
            if (to.HasValue) q = q.Where(t => t.TelemetryDateUtc <= to.Value);
            var total = await q.CountAsync();
            var items = await q.OrderByDescending(t => t.RecordedAtUtc)
                .Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();
            return new PagedResult<MachineTelemetry>(items, page, pageSize, total);
        })
        .WithName("GetMachineTelemetries").WithSummary("List machine telemetry");

        g.MapGet("/{id:long}", async Task<Results<Ok<MachineTelemetry>, NotFound>> (long id, TheMESThingDbContext db) =>
        {
            var e = await db.MachineTelemetries.AsNoTracking()
                .FirstOrDefaultAsync(t => t.MachineTelemetryId == id);
            return e is null ? TypedResults.NotFound() : TypedResults.Ok(e);
        })
        .WithName("GetMachineTelemetry").WithSummary("Get a telemetry record by ID");

        g.MapPost("/", async Task<Created<MachineTelemetry>> (MachineTelemetry body, TheMESThingDbContext db) =>
        {
            db.MachineTelemetries.Add(body);
            await db.SaveChangesAsync();
            return TypedResults.Created($"/api/iot/telemetry/{body.MachineTelemetryId}", body);
        })
        .WithName("CreateMachineTelemetry").WithSummary("Ingest a telemetry record");

        g.MapDelete("/{id:long}", async Task<Results<NoContent, NotFound>> (long id, TheMESThingDbContext db) =>
        {
            var e = await db.MachineTelemetries.FirstOrDefaultAsync(t => t.MachineTelemetryId == id);
            if (e is null) return TypedResults.NotFound();
            db.MachineTelemetries.Remove(e);
            await db.SaveChangesAsync();
            return TypedResults.NoContent();
        })
        .WithName("DeleteMachineTelemetry").WithSummary("Delete a telemetry record");
    }

    // ── MachineEvents ─────────────────────────────────────────────────────────
    static void MapMachineEvents(IEndpointRouteBuilder app)
    {
        var g = app.MapGroup("/api/iot/machine-events").WithTags("IoT – MachineEvents");

        g.MapGet("/", async (TheMESThingDbContext db, Guid? machineId, string? severity, bool? unacknowledged, int page = 1, int pageSize = 50) =>
        {
            var q = db.MachineEvents.AsNoTracking();
            if (machineId.HasValue) q = q.Where(ev => ev.MachineId == machineId.Value);
            if (!string.IsNullOrEmpty(severity)) q = q.Where(ev => ev.Severity == severity);
            if (unacknowledged == true) q = q.Where(ev => ev.AcknowledgedAtUtc == null);
            var total = await q.CountAsync();
            var items = await q.OrderByDescending(ev => ev.OccurredAtUtc)
                .Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();
            return new PagedResult<MachineEvent>(items, page, pageSize, total);
        })
        .WithName("GetMachineEvents").WithSummary("List machine events");

        g.MapGet("/{id:long}", async Task<Results<Ok<MachineEvent>, NotFound>> (long id, TheMESThingDbContext db) =>
        {
            var e = await db.MachineEvents.AsNoTracking()
                .FirstOrDefaultAsync(ev => ev.MachineEventId == id);
            return e is null ? TypedResults.NotFound() : TypedResults.Ok(e);
        })
        .WithName("GetMachineEvent").WithSummary("Get a machine event by ID");

        g.MapPost("/", async Task<Created<MachineEvent>> (MachineEvent body, TheMESThingDbContext db) =>
        {
            db.MachineEvents.Add(body);
            await db.SaveChangesAsync();
            return TypedResults.Created($"/api/iot/machine-events/{body.MachineEventId}", body);
        })
        .WithName("CreateMachineEvent").WithSummary("Create a machine event");

        g.MapPatch("/{id:long}/acknowledge", async Task<Results<Ok<MachineEvent>, NotFound>> (long id, Guid acknowledgedByUserId, TheMESThingDbContext db) =>
        {
            var e = await db.MachineEvents.FirstOrDefaultAsync(ev => ev.MachineEventId == id);
            if (e is null) return TypedResults.NotFound();
            e.AcknowledgedAtUtc = DateTime.UtcNow;
            e.AcknowledgedByUserId = acknowledgedByUserId;
            await db.SaveChangesAsync();
            return TypedResults.Ok(e);
        })
        .WithName("AcknowledgeMachineEvent").WithSummary("Acknowledge a machine event");

        g.MapDelete("/{id:long}", async Task<Results<NoContent, NotFound>> (long id, TheMESThingDbContext db) =>
        {
            var e = await db.MachineEvents.FirstOrDefaultAsync(ev => ev.MachineEventId == id);
            if (e is null) return TypedResults.NotFound();
            db.MachineEvents.Remove(e);
            await db.SaveChangesAsync();
            return TypedResults.NoContent();
        })
        .WithName("DeleteMachineEvent").WithSummary("Delete a machine event");
    }

    // ── MachineCycles ─────────────────────────────────────────────────────────
    static void MapMachineCycles(IEndpointRouteBuilder app)
    {
        var g = app.MapGroup("/api/iot/machine-cycles").WithTags("IoT – MachineCycles");

        g.MapGet("/", async (TheMESThingDbContext db, Guid? machineId, Guid? productionOrderId, DateOnly? from, DateOnly? to, int page = 1, int pageSize = 100) =>
        {
            var q = db.MachineCycles.AsNoTracking();
            if (machineId.HasValue) q = q.Where(c => c.MachineId == machineId.Value);
            if (productionOrderId.HasValue) q = q.Where(c => c.ProductionOrderId == productionOrderId.Value);
            if (from.HasValue) q = q.Where(c => c.CycleDateUtc >= from.Value);
            if (to.HasValue) q = q.Where(c => c.CycleDateUtc <= to.Value);
            var total = await q.CountAsync();
            var items = await q.OrderByDescending(c => c.CycleStartAtUtc)
                .Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();
            return new PagedResult<MachineCycle>(items, page, pageSize, total);
        })
        .WithName("GetMachineCycles").WithSummary("List machine cycles");

        g.MapGet("/{id:long}", async Task<Results<Ok<MachineCycle>, NotFound>> (long id, TheMESThingDbContext db) =>
        {
            var e = await db.MachineCycles.AsNoTracking()
                .FirstOrDefaultAsync(c => c.MachineCycleId == id);
            return e is null ? TypedResults.NotFound() : TypedResults.Ok(e);
        })
        .WithName("GetMachineCycle").WithSummary("Get a machine cycle by ID");

        g.MapPost("/", async Task<Created<MachineCycle>> (MachineCycle body, TheMESThingDbContext db) =>
        {
            db.MachineCycles.Add(body);
            await db.SaveChangesAsync();
            return TypedResults.Created($"/api/iot/machine-cycles/{body.MachineCycleId}", body);
        })
        .WithName("CreateMachineCycle").WithSummary("Create a machine cycle");

        g.MapPut("/{id:long}", async Task<Results<Ok<MachineCycle>, NotFound>> (long id, MachineCycle body, TheMESThingDbContext db) =>
        {
            var e = await db.MachineCycles.FirstOrDefaultAsync(c => c.MachineCycleId == id);
            if (e is null) return TypedResults.NotFound();
            e.GoodQuantity = body.GoodQuantity;
            e.ScrapQuantity = body.ScrapQuantity;
            e.IsRejected = body.IsRejected;
            e.ScrapReasonCode = body.ScrapReasonCode;
            await db.SaveChangesAsync();
            return TypedResults.Ok(e);
        })
        .WithName("UpdateMachineCycle").WithSummary("Update a machine cycle");

        g.MapDelete("/{id:long}", async Task<Results<NoContent, NotFound>> (long id, TheMESThingDbContext db) =>
        {
            var e = await db.MachineCycles.FirstOrDefaultAsync(c => c.MachineCycleId == id);
            if (e is null) return TypedResults.NotFound();
            db.MachineCycles.Remove(e);
            await db.SaveChangesAsync();
            return TypedResults.NoContent();
        })
        .WithName("DeleteMachineCycle").WithSummary("Delete a machine cycle");
    }
}
