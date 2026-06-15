using Microsoft.EntityFrameworkCore;
using TheMESItemsThingLib.Common;
using TheMESItemsThingLib.Services;
using TheMESThingData;
using TheMESThingData.Entities.Mes;

namespace TheMESThingLib.Services;

public sealed class MachineSkillService(TheMESThingDbContext db) : IMachineSkillService
{
    public async Task<PagedResult<MachineSkill>> GetListAsync(Guid? machineId, int page, int pageSize, CancellationToken ct = default)
    {
        var q = db.MachineSkills.AsNoTracking();
        if (machineId.HasValue) q = q.Where(ms => ms.MachineId == machineId.Value);
        var total = await q.CountAsync(ct);
        var items = await q.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(ct);
        return new PagedResult<MachineSkill>(items, page, pageSize, total);
    }

    public Task<MachineSkill?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        db.MachineSkills.AsNoTracking().FirstOrDefaultAsync(ms => ms.MachineSkillId == id, ct);

    public async Task<MachineSkill> CreateAsync(MachineSkill body, CancellationToken ct = default)
    {
        body.MachineSkillId = Guid.Empty;
        db.MachineSkills.Add(body);
        await db.SaveChangesAsync(ct);
        return body;
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var e = await db.MachineSkills.FindAsync([id], ct);
        if (e is null) return false;
        db.MachineSkills.Remove(e);
        await db.SaveChangesAsync(ct);
        return true;
    }
}
