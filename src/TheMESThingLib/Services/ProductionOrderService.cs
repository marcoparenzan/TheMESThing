using Microsoft.EntityFrameworkCore;
using TheMESItemsThingLib.Common;
using TheMESItemsThingLib.Services;
using TheMESThingData;
using TheMESThingData.Entities.Mes;

namespace TheMESThingLib.Services;

public sealed class ProductionOrderService(TheMESThingDbContext db) : IProductionOrderService
{
    public async Task<PagedResult<ProductionOrder>> GetListAsync(Guid? tenantId, Guid? machineId, string? status, int page, int pageSize, CancellationToken ct = default)
    {
        var q = db.ProductionOrders.AsNoTracking();
        if (tenantId.HasValue) q = q.Where(po => po.TenantId == tenantId.Value);
        if (machineId.HasValue) q = q.Where(po => po.MachineId == machineId.Value);
        if (!string.IsNullOrEmpty(status)) q = q.Where(po => po.Status == status);
        var total = await q.CountAsync(ct);
        var items = await q.OrderByDescending(po => po.PlannedStartAtUtc)
            .Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(ct);
        return new PagedResult<ProductionOrder>(items, page, pageSize, total);
    }

    public Task<ProductionOrder?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        db.ProductionOrders.AsNoTracking().FirstOrDefaultAsync(po => po.ProductionOrderId == id, ct);

    public async Task<ProductionOrder> CreateAsync(ProductionOrder body, CancellationToken ct = default)
    {
        body.ProductionOrderId = Guid.Empty;
        db.ProductionOrders.Add(body);
        await db.SaveChangesAsync(ct);
        return body;
    }

    public async Task<ProductionOrder?> UpdateAsync(Guid id, ProductionOrder body, CancellationToken ct = default)
    {
        var e = await db.ProductionOrders.FindAsync([id], ct);
        if (e is null) return null;
        e.Status = body.Status;
        e.MachineId = body.MachineId;
        e.ProductionLineId = body.ProductionLineId;
        e.OperationSequence = body.OperationSequence;
        e.PlannedQuantity = body.PlannedQuantity;
        e.GoodQuantity = body.GoodQuantity;
        e.ScrapQuantity = body.ScrapQuantity;
        e.ReworkQuantity = body.ReworkQuantity;
        e.PlannedStartAtUtc = body.PlannedStartAtUtc;
        e.PlannedEndAtUtc = body.PlannedEndAtUtc;
        e.ActualStartAtUtc = body.ActualStartAtUtc;
        e.ActualEndAtUtc = body.ActualEndAtUtc;
        e.PlannedCycleTimeSeconds = body.PlannedCycleTimeSeconds;
        e.ActualCycleTimeSeconds = body.ActualCycleTimeSeconds;
        e.UpdatedAtUtc = DateTime.UtcNow;
        await db.SaveChangesAsync(ct);
        return e;
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var e = await db.ProductionOrders.FindAsync([id], ct);
        if (e is null) return false;
        db.ProductionOrders.Remove(e);
        await db.SaveChangesAsync(ct);
        return true;
    }
}
