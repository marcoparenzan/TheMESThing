using Microsoft.EntityFrameworkCore;
using TheMESItemsThingLib.Common;
using TheMESItemsThingLib.Services;
using TheMESThingData;
using TheMESThingData.Entities.Mes;

namespace TheMESThingLib.Services;

public sealed class ProductService(TheMESThingDbContext db) : IProductService
{
    public async Task<PagedResult<Product>> GetListAsync(Guid? tenantId, int page, int pageSize, CancellationToken ct = default)
    {
        var q = db.Products.AsNoTracking();
        if (tenantId.HasValue) q = q.Where(p => p.TenantId == tenantId.Value);
        var total = await q.CountAsync(ct);
        var items = await q.OrderBy(p => p.ProductCode)
            .Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(ct);
        return new PagedResult<Product>(items, page, pageSize, total);
    }

    public Task<Product?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        db.Products.AsNoTracking().FirstOrDefaultAsync(p => p.ProductId == id, ct);

    public async Task<Product> CreateAsync(Product body, CancellationToken ct = default)
    {
        body.ProductId = Guid.Empty;
        db.Products.Add(body);
        await db.SaveChangesAsync(ct);
        return body;
    }

    public async Task<Product?> UpdateAsync(Guid id, Product body, CancellationToken ct = default)
    {
        var e = await db.Products.FindAsync([id], ct);
        if (e is null) return null;
        e.ProductCode = body.ProductCode;
        e.ProductName = body.ProductName;
        e.UnitOfMeasure = body.UnitOfMeasure;
        e.CycleTimeSeconds = body.CycleTimeSeconds;
        e.SetupTimeSeconds = body.SetupTimeSeconds;
        e.IsActive = body.IsActive;
        e.UpdatedAtUtc = DateTime.UtcNow;
        await db.SaveChangesAsync(ct);
        return e;
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var e = await db.Products.FindAsync([id], ct);
        if (e is null) return false;
        db.Products.Remove(e);
        await db.SaveChangesAsync(ct);
        return true;
    }
}
