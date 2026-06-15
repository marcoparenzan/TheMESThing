using TheMESItemsThingLib.Common;
using TheMESThingData.Entities.Mes;

namespace TheMESItemsThingLib.Services;

public interface ISkillService
{
    Task<PagedResult<Skill>> GetListAsync(Guid? tenantId, int page, int pageSize, CancellationToken ct = default);
    Task<Skill?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<Skill> CreateAsync(Skill body, CancellationToken ct = default);
    Task<Skill?> UpdateAsync(Guid id, Skill body, CancellationToken ct = default);
    Task<bool> DeleteAsync(Guid id, CancellationToken ct = default);
}
