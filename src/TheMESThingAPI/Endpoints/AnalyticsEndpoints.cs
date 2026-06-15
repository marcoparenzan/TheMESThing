using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using TheMESThingAPI.Common;
using TheMESThingData;
using TheMESThingData.Entities.Analytics;

namespace TheMESThingAPI.Endpoints;

public static class AnalyticsEndpoints
{
    public static IEndpointRouteBuilder MapAnalyticsEndpoints(this IEndpointRouteBuilder app)
    {
        MapKpiDefinitions(app);
        MapKpiValues(app);
        MapOeeCalculations(app);
        MapShiftReports(app);
        return app;
    }

    // ── KpiDefinitions ────────────────────────────────────────────────────────
    static void MapKpiDefinitions(IEndpointRouteBuilder app)
    {
        var g = app.MapGroup("/api/analytics/kpi-definitions").WithTags("Analytics – KpiDefinitions");

        g.MapGet("/", async (TheMESThingDbContext db, int page = 1, int pageSize = 50) =>
        {
            var total = await db.KpiDefinitions.CountAsync();
            var items = await db.KpiDefinitions.AsNoTracking()
                .OrderBy(k => k.KpiGroup).ThenBy(k => k.KpiCode)
                .Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();
            return new PagedResult<KpiDefinition>(items, page, pageSize, total);
        })
        .WithName("GetKpiDefinitions").WithSummary("List KPI definitions");

        g.MapGet("/{id:guid}", async Task<Results<Ok<KpiDefinition>, NotFound>> (Guid id, TheMESThingDbContext db) =>
        {
            var e = await db.KpiDefinitions.AsNoTracking().FirstOrDefaultAsync(k => k.KpiDefinitionId == id);
            return e is null ? TypedResults.NotFound() : TypedResults.Ok(e);
        })
        .WithName("GetKpiDefinition").WithSummary("Get a KPI definition by ID");

        g.MapPost("/", async Task<Created<KpiDefinition>> (KpiDefinition body, TheMESThingDbContext db) =>
        {
            body.KpiDefinitionId = Guid.Empty;
            db.KpiDefinitions.Add(body);
            await db.SaveChangesAsync();
            return TypedResults.Created($"/api/analytics/kpi-definitions/{body.KpiDefinitionId}", body);
        })
        .WithName("CreateKpiDefinition").WithSummary("Create a KPI definition");

        g.MapPut("/{id:guid}", async Task<Results<Ok<KpiDefinition>, NotFound>> (Guid id, KpiDefinition body, TheMESThingDbContext db) =>
        {
            var e = await db.KpiDefinitions.FindAsync(id);
            if (e is null) return TypedResults.NotFound();
            e.KpiCode = body.KpiCode;
            e.KpiName = body.KpiName;
            e.KpiGroup = body.KpiGroup;
            e.IsoReference = body.IsoReference;
            e.Formula = body.Formula;
            e.Unit = body.Unit;
            e.TargetValue = body.TargetValue;
            e.WarningThreshold = body.WarningThreshold;
            e.CriticalThreshold = body.CriticalThreshold;
            e.IsHigherBetter = body.IsHigherBetter;
            e.Description = body.Description;
            await db.SaveChangesAsync();
            return TypedResults.Ok(e);
        })
        .WithName("UpdateKpiDefinition").WithSummary("Update a KPI definition");

        g.MapDelete("/{id:guid}", async Task<Results<NoContent, NotFound>> (Guid id, TheMESThingDbContext db) =>
        {
            var e = await db.KpiDefinitions.FindAsync(id);
            if (e is null) return TypedResults.NotFound();
            db.KpiDefinitions.Remove(e);
            await db.SaveChangesAsync();
            return TypedResults.NoContent();
        })
        .WithName("DeleteKpiDefinition").WithSummary("Delete a KPI definition");
    }

    // ── KpiValues ─────────────────────────────────────────────────────────────
    static void MapKpiValues(IEndpointRouteBuilder app)
    {
        var g = app.MapGroup("/api/analytics/kpi-values").WithTags("Analytics – KpiValues");

        g.MapGet("/", async (TheMESThingDbContext db, Guid? tenantId, Guid? kpiDefinitionId, Guid? machineId, DateOnly? from, DateOnly? to, int page = 1, int pageSize = 100) =>
        {
            var q = db.KpiValues.AsNoTracking();
            if (tenantId.HasValue) q = q.Where(v => v.TenantId == tenantId.Value);
            if (kpiDefinitionId.HasValue) q = q.Where(v => v.KpiDefinitionId == kpiDefinitionId.Value);
            if (machineId.HasValue) q = q.Where(v => v.MachineId == machineId.Value);
            if (from.HasValue) q = q.Where(v => v.PeriodDate >= from.Value);
            if (to.HasValue) q = q.Where(v => v.PeriodDate <= to.Value);
            var total = await q.CountAsync();
            var items = await q.OrderByDescending(v => v.PeriodDate)
                .Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();
            return new PagedResult<KpiValue>(items, page, pageSize, total);
        })
        .WithName("GetKpiValues").WithSummary("List KPI values");

        g.MapGet("/{id:long}", async Task<Results<Ok<KpiValue>, NotFound>> (long id, TheMESThingDbContext db) =>
        {
            var e = await db.KpiValues.AsNoTracking().FirstOrDefaultAsync(v => v.KpiValueId == id);
            return e is null ? TypedResults.NotFound() : TypedResults.Ok(e);
        })
        .WithName("GetKpiValue").WithSummary("Get a KPI value by ID");

        g.MapPost("/", async Task<Created<KpiValue>> (KpiValue body, TheMESThingDbContext db) =>
        {
            db.KpiValues.Add(body);
            await db.SaveChangesAsync();
            return TypedResults.Created($"/api/analytics/kpi-values/{body.KpiValueId}", body);
        })
        .WithName("CreateKpiValue").WithSummary("Record a KPI value");

        g.MapDelete("/{id:long}", async Task<Results<NoContent, NotFound>> (long id, TheMESThingDbContext db) =>
        {
            var e = await db.KpiValues.FindAsync(id);
            if (e is null) return TypedResults.NotFound();
            db.KpiValues.Remove(e);
            await db.SaveChangesAsync();
            return TypedResults.NoContent();
        })
        .WithName("DeleteKpiValue").WithSummary("Delete a KPI value");
    }

    // ── OeeCalculations ───────────────────────────────────────────────────────
    static void MapOeeCalculations(IEndpointRouteBuilder app)
    {
        var g = app.MapGroup("/api/analytics/oee").WithTags("Analytics – OeeCalculations");

        g.MapGet("/", async (TheMESThingDbContext db, Guid? tenantId, Guid? machineId, DateOnly? from, DateOnly? to, string? periodType, int page = 1, int pageSize = 100) =>
        {
            var q = db.OeeCalculations.AsNoTracking();
            if (tenantId.HasValue) q = q.Where(o => o.TenantId == tenantId.Value);
            if (machineId.HasValue) q = q.Where(o => o.MachineId == machineId.Value);
            if (!string.IsNullOrEmpty(periodType)) q = q.Where(o => o.PeriodType == periodType);
            if (from.HasValue) q = q.Where(o => o.PeriodDate >= from.Value);
            if (to.HasValue) q = q.Where(o => o.PeriodDate <= to.Value);
            var total = await q.CountAsync();
            var items = await q.OrderByDescending(o => o.PeriodDate)
                .Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();
            return new PagedResult<OeeCalculation>(items, page, pageSize, total);
        })
        .WithName("GetOeeCalculations").WithSummary("List OEE calculations");

        g.MapGet("/{id:long}", async Task<Results<Ok<OeeCalculation>, NotFound>> (long id, TheMESThingDbContext db) =>
        {
            var e = await db.OeeCalculations.AsNoTracking().FirstOrDefaultAsync(o => o.OeeCalculationId == id);
            return e is null ? TypedResults.NotFound() : TypedResults.Ok(e);
        })
        .WithName("GetOeeCalculation").WithSummary("Get an OEE calculation by ID");

        g.MapPost("/", async Task<Created<OeeCalculation>> (OeeCalculation body, TheMESThingDbContext db) =>
        {
            db.OeeCalculations.Add(body);
            await db.SaveChangesAsync();
            return TypedResults.Created($"/api/analytics/oee/{body.OeeCalculationId}", body);
        })
        .WithName("CreateOeeCalculation").WithSummary("Store an OEE calculation");

        g.MapPut("/{id:long}", async Task<Results<Ok<OeeCalculation>, NotFound>> (long id, OeeCalculation body, TheMESThingDbContext db) =>
        {
            var e = await db.OeeCalculations.FindAsync(id);
            if (e is null) return TypedResults.NotFound();
            e.Availability = body.Availability;
            e.Performance = body.Performance;
            e.Quality = body.Quality;
            e.OeeValue = body.OeeValue;
            e.GoodQuantity = body.GoodQuantity;
            e.TotalQuantity = body.TotalQuantity;
            e.RunTimeSeconds = body.RunTimeSeconds;
            e.OperatingTimeSeconds = body.OperatingTimeSeconds;
            e.UnscheduledDowntimeSeconds = body.UnscheduledDowntimeSeconds;
            e.ScheduledDowntimeSeconds = body.ScheduledDowntimeSeconds;
            e.MeanTimeBetweenFailureSeconds = body.MeanTimeBetweenFailureSeconds;
            e.MeanTimeToRepairSeconds = body.MeanTimeToRepairSeconds;
            await db.SaveChangesAsync();
            return TypedResults.Ok(e);
        })
        .WithName("UpdateOeeCalculation").WithSummary("Update an OEE calculation");

        g.MapDelete("/{id:long}", async Task<Results<NoContent, NotFound>> (long id, TheMESThingDbContext db) =>
        {
            var e = await db.OeeCalculations.FindAsync(id);
            if (e is null) return TypedResults.NotFound();
            db.OeeCalculations.Remove(e);
            await db.SaveChangesAsync();
            return TypedResults.NoContent();
        })
        .WithName("DeleteOeeCalculation").WithSummary("Delete an OEE calculation");
    }

    // ── ShiftReports ──────────────────────────────────────────────────────────
    static void MapShiftReports(IEndpointRouteBuilder app)
    {
        var g = app.MapGroup("/api/analytics/shift-reports").WithTags("Analytics – ShiftReports");

        g.MapGet("/", async (TheMESThingDbContext db, Guid? tenantId, Guid? machineId, DateOnly? from, DateOnly? to, int page = 1, int pageSize = 50) =>
        {
            var q = db.ShiftReports.AsNoTracking();
            if (tenantId.HasValue) q = q.Where(sr => sr.TenantId == tenantId.Value);
            if (machineId.HasValue) q = q.Where(sr => sr.MachineId == machineId.Value);
            if (from.HasValue) q = q.Where(sr => sr.ShiftDate >= from.Value);
            if (to.HasValue) q = q.Where(sr => sr.ShiftDate <= to.Value);
            var total = await q.CountAsync();
            var items = await q.OrderByDescending(sr => sr.ShiftDate)
                .Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();
            return new PagedResult<ShiftReport>(items, page, pageSize, total);
        })
        .WithName("GetShiftReports").WithSummary("List shift reports");

        g.MapGet("/{id:long}", async Task<Results<Ok<ShiftReport>, NotFound>> (long id, TheMESThingDbContext db) =>
        {
            var e = await db.ShiftReports.AsNoTracking().FirstOrDefaultAsync(sr => sr.ShiftReportId == id);
            return e is null ? TypedResults.NotFound() : TypedResults.Ok(e);
        })
        .WithName("GetShiftReport").WithSummary("Get a shift report by ID");

        g.MapPost("/", async Task<Created<ShiftReport>> (ShiftReport body, TheMESThingDbContext db) =>
        {
            db.ShiftReports.Add(body);
            await db.SaveChangesAsync();
            return TypedResults.Created($"/api/analytics/shift-reports/{body.ShiftReportId}", body);
        })
        .WithName("CreateShiftReport").WithSummary("Create a shift report");

        g.MapPut("/{id:long}", async Task<Results<Ok<ShiftReport>, NotFound>> (long id, ShiftReport body, TheMESThingDbContext db) =>
        {
            var e = await db.ShiftReports.FindAsync(id);
            if (e is null) return TypedResults.NotFound();
            e.ShiftEndAtUtc = body.ShiftEndAtUtc;
            e.TotalRunTimeSeconds = body.TotalRunTimeSeconds;
            e.TotalDownTimeSeconds = body.TotalDownTimeSeconds;
            e.TotalIdleTimeSeconds = body.TotalIdleTimeSeconds;
            e.GoodQuantity = body.GoodQuantity;
            e.ScrapQuantity = body.ScrapQuantity;
            e.TotalCycles = body.TotalCycles;
            e.OeeValue = body.OeeValue;
            e.Notes = body.Notes;
            await db.SaveChangesAsync();
            return TypedResults.Ok(e);
        })
        .WithName("UpdateShiftReport").WithSummary("Update a shift report");

        g.MapDelete("/{id:long}", async Task<Results<NoContent, NotFound>> (long id, TheMESThingDbContext db) =>
        {
            var e = await db.ShiftReports.FindAsync(id);
            if (e is null) return TypedResults.NotFound();
            db.ShiftReports.Remove(e);
            await db.SaveChangesAsync();
            return TypedResults.NoContent();
        })
        .WithName("DeleteShiftReport").WithSummary("Delete a shift report");
    }
}
