using TheMESItemsThingLib.Common;
using TheMESThingData.Entities.Mes;

namespace TheMESItemsThingLib.Services;

public interface IProductionOrderService
{
    Task<PagedResult<ProductionOrder>> GetListAsync(Guid? tenantId, Guid? machineId, string? status, int page, int pageSize, CancellationToken ct = default);
    Task<ProductionOrder?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<ProductionOrder> CreateAsync(ProductionOrder body, CancellationToken ct = default);
    Task<ProductionOrder?> UpdateAsync(Guid id, ProductionOrder body, CancellationToken ct = default);
    Task<bool> DeleteAsync(Guid id, CancellationToken ct = default);
}
