using TheMESItemsThingLib.Common;
using TheMESThingData.Entities.Mes;

namespace TheMESItemsThingLib.Services;

public interface IOperatorSkillService
{
    Task<PagedResult<OperatorSkill>> GetListAsync(Guid? operatorId, int page, int pageSize, CancellationToken ct = default);
    Task<OperatorSkill?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<OperatorSkill> CreateAsync(OperatorSkill body, CancellationToken ct = default);
    Task<OperatorSkill?> UpdateAsync(Guid id, OperatorSkill body, CancellationToken ct = default);
    Task<bool> DeleteAsync(Guid id, CancellationToken ct = default);
}
