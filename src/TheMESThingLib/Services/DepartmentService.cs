using Microsoft.EntityFrameworkCore;
using TheMESItemsThingLib.Common;
using TheMESItemsThingLib.Services;
using TheMESThingData;
using TheMESThingData.Entities.Mes;

namespace TheMESThingLib.Services;

public sealed class DepartmentService(TheMESThingDbContext db) : IDepartmentService
{
    public async Task<PagedResult<Department>> GetListAsync(Guid? tenantId, int page, int pageSize, CancellationToken ct = default)
    {
        var q = db.Departments.AsNoTracking();
        if (tenantId.HasValue) q = q.Where(d => d.TenantId == tenantId.Value);
        var total = await q.CountAsync(ct);
        var items = await q.OrderBy(d => d.DepartmentName)
            .Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(ct);
        return new PagedResult<Department>(items, page, pageSize, total);
    }

    public Task<Department?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        db.Departments.AsNoTracking().FirstOrDefaultAsync(d => d.DepartmentId == id, ct);

    public async Task<Department> CreateAsync(Department body, CancellationToken ct = default)
    {
        body.DepartmentId = Guid.Empty;
        db.Departments.Add(body);
        await db.SaveChangesAsync(ct);
        return body;
    }

    public async Task<Department?> UpdateAsync(Guid id, Department body, CancellationToken ct = default)
    {
        var e = await db.Departments.FindAsync([id], ct);
        if (e is null) return null;
        e.DepartmentCode = body.DepartmentCode;
        e.DepartmentName = body.DepartmentName;
        e.ParentDepartmentId = body.ParentDepartmentId;
        e.ManagerUserId = body.ManagerUserId;
        e.CostCenter = body.CostCenter;
        e.ExternalMicrosoft365Id = body.ExternalMicrosoft365Id;
        e.IsActive = body.IsActive;
        e.UpdatedAtUtc = DateTime.UtcNow;
        await db.SaveChangesAsync(ct);
        return e;
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var e = await db.Departments.FindAsync([id], ct);
        if (e is null) return false;
        db.Departments.Remove(e);
        await db.SaveChangesAsync(ct);
        return true;
    }
}
