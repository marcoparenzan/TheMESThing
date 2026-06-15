using Microsoft.EntityFrameworkCore;
using TheMESItemsThingLib.Common;
using TheMESItemsThingLib.Services;
using TheMESThingData;
using TheMESThingData.Entities.Mes;

namespace TheMESThingLib.Services;

public sealed class CustomerService(TheMESThingDbContext db) : ICustomerService
{
    public async Task<PagedResult<Customer>> GetListAsync(Guid? tenantId, int page, int pageSize, CancellationToken ct = default)
    {
        var q = db.Customers.AsNoTracking();
        if (tenantId.HasValue) q = q.Where(c => c.TenantId == tenantId.Value);
        var total = await q.CountAsync(ct);
        var items = await q.OrderBy(c => c.CustomerName)
            .Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(ct);
        return new PagedResult<Customer>(items, page, pageSize, total);
    }

    public Task<Customer?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        db.Customers.AsNoTracking().FirstOrDefaultAsync(c => c.CustomerId == id, ct);

    public async Task<Customer> CreateAsync(Customer body, CancellationToken ct = default)
    {
        body.CustomerId = Guid.Empty;
        db.Customers.Add(body);
        await db.SaveChangesAsync(ct);
        return body;
    }

    public async Task<Customer?> UpdateAsync(Guid id, Customer body, CancellationToken ct = default)
    {
        var e = await db.Customers.FindAsync([id], ct);
        if (e is null) return null;
        e.CustomerCode = body.CustomerCode;
        e.CustomerName = body.CustomerName;
        e.ContactEmail = body.ContactEmail;
        e.ContactPhone = body.ContactPhone;
        e.Address = body.Address;
        e.Country = body.Country;
        e.ExternalMicrosoft365Id = body.ExternalMicrosoft365Id;
        e.IsActive = body.IsActive;
        e.UpdatedAtUtc = DateTime.UtcNow;
        await db.SaveChangesAsync(ct);
        return e;
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var e = await db.Customers.FindAsync([id], ct);
        if (e is null) return false;
        db.Customers.Remove(e);
        await db.SaveChangesAsync(ct);
        return true;
    }
}
