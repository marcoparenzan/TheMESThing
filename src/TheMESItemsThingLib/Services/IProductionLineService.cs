using TheMESItemsThingLib.Common;
using TheMESThingData.Entities.Mes;

namespace TheMESItemsThingLib.Services;

public interface IProductionLineService
{
    Task<PagedResult<ProductionLine>> GetListAsync(Guid? tenantId, int page, int pageSize, CancellationToken ct = default);
    Task<ProductionLine?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<ProductionLine> CreateAsync(ProductionLine body, CancellationToken ct = default);
    Task<ProductionLine?> UpdateAsync(Guid id, ProductionLine body, CancellationToken ct = default);
    Task<bool> DeleteAsync(Guid id, CancellationToken ct = default);
}
