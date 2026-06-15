using TheMESItemsThingLib.Common;
using TheMESThingData.Entities.Mes;

namespace TheMESItemsThingLib.Services;

public interface IMachineSkillService
{
    Task<PagedResult<MachineSkill>> GetListAsync(Guid? machineId, int page, int pageSize, CancellationToken ct = default);
    Task<MachineSkill?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<MachineSkill> CreateAsync(MachineSkill body, CancellationToken ct = default);
    Task<bool> DeleteAsync(Guid id, CancellationToken ct = default);
}
