using Microsoft.EntityFrameworkCore;
using TheMESItemsThingLib.Common;
using TheMESItemsThingLib.Services;
using TheMESThingData;
using TheMESThingData.Entities.Mes;

namespace TheMESThingLib.Services;

public sealed class ShiftService(TheMESThingDbContext db) : IShiftService
{
    public async Task<PagedResult<Shift>> GetListAsync(Guid? tenantId, int page, int pageSize, CancellationToken ct = default)
    {
        var q = db.Shifts.AsNoTracking();
        if (tenantId.HasValue) q = q.Where(s => s.TenantId == tenantId.Value);
        var total = await q.CountAsync(ct);
        var items = await q.OrderBy(s => s.ShiftCode)
            .Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(ct);
        return new PagedResult<Shift>(items, page, pageSize, total);
    }

    public Task<Shift?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        db.Shifts.AsNoTracking().FirstOrDefaultAsync(s => s.ShiftId == id, ct);

    public async Task<Shift> CreateAsync(Shift body, CancellationToken ct = default)
    {
        body.ShiftId = Guid.Empty;
        db.Shifts.Add(body);
        await db.SaveChangesAsync(ct);
        return body;
    }

    public async Task<Shift?> UpdateAsync(Guid id, Shift body, CancellationToken ct = default)
    {
        var e = await db.Shifts.FindAsync([id], ct);
        if (e is null) return null;
        e.ShiftCode = body.ShiftCode;
        e.ShiftName = body.ShiftName;
        e.StartTime = body.StartTime;
        e.EndTime = body.EndTime;
        e.BreakMinutes = body.BreakMinutes;
        e.IsNightShift = body.IsNightShift;
        e.IsActive = body.IsActive;
        await db.SaveChangesAsync(ct);
        return e;
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var e = await db.Shifts.FindAsync([id], ct);
        if (e is null) return false;
        db.Shifts.Remove(e);
        await db.SaveChangesAsync(ct);
        return true;
    }
}
