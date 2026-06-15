using TheMESItemsThingLib.Common;
using TheMESThingData.Entities.Mes;

namespace TheMESItemsThingLib.Services;

public interface IWorkOrderService
{
    Task<PagedResult<WorkOrder>> GetListAsync(Guid? tenantId, string? status, int page, int pageSize, CancellationToken ct = default);
    Task<WorkOrder?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<WorkOrder> CreateAsync(WorkOrder body, CancellationToken ct = default);
    Task<WorkOrder?> UpdateAsync(Guid id, WorkOrder body, CancellationToken ct = default);
    Task<bool> DeleteAsync(Guid id, CancellationToken ct = default);
}
