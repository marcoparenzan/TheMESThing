using TheMESItemsThingLib.Common;
using TheMESThingData.Entities.Mes;

namespace TheMESItemsThingLib.Services;

public interface IProductService
{
    Task<PagedResult<Product>> GetListAsync(Guid? tenantId, int page, int pageSize, CancellationToken ct = default);
    Task<Product?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<Product> CreateAsync(Product body, CancellationToken ct = default);
    Task<Product?> UpdateAsync(Guid id, Product body, CancellationToken ct = default);
    Task<bool> DeleteAsync(Guid id, CancellationToken ct = default);
}
