using Microsoft.EntityFrameworkCore;
using TheMESItemsThingLib.Common;
using TheMESItemsThingLib.Services;
using TheMESThingData;
using TheMESThingData.Entities.Mes;

namespace TheMESThingLib.Services;

public sealed class OperatorService(TheMESThingDbContext db) : IOperatorService
{
    public async Task<PagedResult<Operator>> GetListAsync(Guid? tenantId, Guid? departmentId, int page, int pageSize, CancellationToken ct = default)
    {
        var q = db.Operators.AsNoTracking();
        if (tenantId.HasValue) q = q.Where(o => o.TenantId == tenantId.Value);
        if (departmentId.HasValue) q = q.Where(o => o.DepartmentId == departmentId.Value);
        var total = await q.CountAsync(ct);
        var items = await q.OrderBy(o => o.LastName).ThenBy(o => o.FirstName)
            .Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(ct);
        return new PagedResult<Operator>(items, page, pageSize, total);
    }

    public Task<Operator?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        db.Operators.AsNoTracking().FirstOrDefaultAsync(o => o.OperatorId == id, ct);

    public async Task<Operator> CreateAsync(Operator body, CancellationToken ct = default)
    {
        body.OperatorId = Guid.Empty;
        db.Operators.Add(body);
        await db.SaveChangesAsync(ct);
        return body;
    }

    public async Task<Operator?> UpdateAsync(Guid id, Operator body, CancellationToken ct = default)
    {
        var e = await db.Operators.FindAsync([id], ct);
        if (e is null) return null;
        e.OperatorCode = body.OperatorCode;
        e.FirstName = body.FirstName;
        e.LastName = body.LastName;
        e.BadgeNumber = body.BadgeNumber;
        e.DepartmentId = body.DepartmentId;
        e.ExternalMicrosoft365Id = body.ExternalMicrosoft365Id;
        e.IsActive = body.IsActive;
        e.HireDate = body.HireDate;
        e.UpdatedAtUtc = DateTime.UtcNow;
        await db.SaveChangesAsync(ct);
        return e;
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var e = await db.Operators.FindAsync([id], ct);
        if (e is null) return false;
        db.Operators.Remove(e);
        await db.SaveChangesAsync(ct);
        return true;
    }
}
