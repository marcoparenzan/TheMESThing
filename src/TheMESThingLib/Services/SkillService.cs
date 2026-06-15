using Microsoft.EntityFrameworkCore;
using TheMESItemsThingLib.Common;
using TheMESItemsThingLib.Services;
using TheMESThingData;
using TheMESThingData.Entities.Mes;

namespace TheMESThingLib.Services;

public sealed class SkillService(TheMESThingDbContext db) : ISkillService
{
    public async Task<PagedResult<Skill>> GetListAsync(Guid? tenantId, int page, int pageSize, CancellationToken ct = default)
    {
        var q = db.Skills.AsNoTracking();
        if (tenantId.HasValue) q = q.Where(s => s.TenantId == tenantId.Value);
        var total = await q.CountAsync(ct);
        var items = await q.OrderBy(s => s.SkillName)
            .Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(ct);
        return new PagedResult<Skill>(items, page, pageSize, total);
    }

    public Task<Skill?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        db.Skills.AsNoTracking().FirstOrDefaultAsync(s => s.SkillId == id, ct);

    public async Task<Skill> CreateAsync(Skill body, CancellationToken ct = default)
    {
        body.SkillId = Guid.Empty;
        db.Skills.Add(body);
        await db.SaveChangesAsync(ct);
        return body;
    }

    public async Task<Skill?> UpdateAsync(Guid id, Skill body, CancellationToken ct = default)
    {
        var e = await db.Skills.FindAsync([id], ct);
        if (e is null) return null;
        e.SkillCode = body.SkillCode;
        e.SkillName = body.SkillName;
        e.Category = body.Category;
        e.Description = body.Description;
        e.IsActive = body.IsActive;
        await db.SaveChangesAsync(ct);
        return e;
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var e = await db.Skills.FindAsync([id], ct);
        if (e is null) return false;
        db.Skills.Remove(e);
        await db.SaveChangesAsync(ct);
        return true;
    }
}
