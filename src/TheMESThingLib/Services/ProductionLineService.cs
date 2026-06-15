using Microsoft.EntityFrameworkCore;
using TheMESItemsThingLib.Common;
using TheMESItemsThingLib.Services;
using TheMESThingData;
using TheMESThingData.Entities.Mes;

namespace TheMESThingLib.Services;

public sealed class ProductionLineService(TheMESThingDbContext db) : IProductionLineService
{
    public async Task<PagedResult<ProductionLine>> GetListAsync(Guid? tenantId, int page, int pageSize, CancellationToken ct = default)
    {
        var q = db.ProductionLines.AsNoTracking();
        if (tenantId.HasValue) q = q.Where(l => l.TenantId == tenantId.Value);
        var total = await q.CountAsync(ct);
        var items = await q.OrderBy(l => l.LineName)
            .Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(ct);
        return new PagedResult<ProductionLine>(items, page, pageSize, total);
    }

    public Task<ProductionLine?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        db.ProductionLines.AsNoTracking().FirstOrDefaultAsync(l => l.ProductionLineId == id, ct);

    public async Task<ProductionLine> CreateAsync(ProductionLine body, CancellationToken ct = default)
    {
        body.ProductionLineId = Guid.Empty;
        db.ProductionLines.Add(body);
        await db.SaveChangesAsync(ct);
        return body;
    }

    public async Task<ProductionLine?> UpdateAsync(Guid id, ProductionLine body, CancellationToken ct = default)
    {
        var e = await db.ProductionLines.FindAsync([id], ct);
        if (e is null) return null;
        e.LineCode = body.LineCode;
        e.LineName = body.LineName;
        e.LineType = body.LineType;
        e.NominalCapacity = body.NominalCapacity;
        e.CapacityUnit = body.CapacityUnit;
        e.IsActive = body.IsActive;
        e.UpdatedAtUtc = DateTime.UtcNow;
        await db.SaveChangesAsync(ct);
        return e;
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var e = await db.ProductionLines.FindAsync([id], ct);
        if (e is null) return false;
        db.ProductionLines.Remove(e);
        await db.SaveChangesAsync(ct);
        return true;
    }
}
