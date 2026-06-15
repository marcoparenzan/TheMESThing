using Microsoft.EntityFrameworkCore;
using TheMESItemsThingLib.Common;
using TheMESItemsThingLib.Services;
using TheMESThingData;
using TheMESThingData.Entities.Mes;

namespace TheMESThingLib.Services;

public sealed class OperatorSkillService(TheMESThingDbContext db) : IOperatorSkillService
{
    public async Task<PagedResult<OperatorSkill>> GetListAsync(Guid? operatorId, int page, int pageSize, CancellationToken ct = default)
    {
        var q = db.OperatorSkills.AsNoTracking();
        if (operatorId.HasValue) q = q.Where(os => os.OperatorId == operatorId.Value);
        var total = await q.CountAsync(ct);
        var items = await q.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(ct);
        return new PagedResult<OperatorSkill>(items, page, pageSize, total);
    }

    public Task<OperatorSkill?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        db.OperatorSkills.AsNoTracking().FirstOrDefaultAsync(os => os.OperatorSkillId == id, ct);

    public async Task<OperatorSkill> CreateAsync(OperatorSkill body, CancellationToken ct = default)
    {
        body.OperatorSkillId = Guid.Empty;
        db.OperatorSkills.Add(body);
        await db.SaveChangesAsync(ct);
        return body;
    }

    public async Task<OperatorSkill?> UpdateAsync(Guid id, OperatorSkill body, CancellationToken ct = default)
    {
        var e = await db.OperatorSkills.FindAsync([id], ct);
        if (e is null) return null;
        e.ProficiencyLevel = body.ProficiencyLevel;
        e.CertifiedAtUtc = body.CertifiedAtUtc;
        e.ExpiresAtUtc = body.ExpiresAtUtc;
        await db.SaveChangesAsync(ct);
        return e;
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var e = await db.OperatorSkills.FindAsync([id], ct);
        if (e is null) return false;
        db.OperatorSkills.Remove(e);
        await db.SaveChangesAsync(ct);
        return true;
    }
}
