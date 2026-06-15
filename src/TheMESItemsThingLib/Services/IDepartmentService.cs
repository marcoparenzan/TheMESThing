using TheMESItemsThingLib.Common;
using TheMESThingData.Entities.Mes;

namespace TheMESItemsThingLib.Services;

public interface IDepartmentService
{
    Task<PagedResult<Department>> GetListAsync(Guid? tenantId, int page, int pageSize, CancellationToken ct = default);
    Task<Department?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<Department> CreateAsync(Department body, CancellationToken ct = default);
    Task<Department?> UpdateAsync(Guid id, Department body, CancellationToken ct = default);
    Task<bool> DeleteAsync(Guid id, CancellationToken ct = default);
}
