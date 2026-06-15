using TheMESItemsThingLib.Common;
using TheMESThingData.Entities.Mes;

namespace TheMESItemsThingLib.Services;

public interface IMachineService
{
    Task<PagedResult<Machine>> GetListAsync(Guid? tenantId, Guid? departmentId, int page, int pageSize, CancellationToken ct = default);
    Task<Machine?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<Machine> CreateAsync(Machine body, CancellationToken ct = default);
    Task<Machine?> UpdateAsync(Guid id, Machine body, CancellationToken ct = default);
    Task<bool> DeleteAsync(Guid id, CancellationToken ct = default);
}
