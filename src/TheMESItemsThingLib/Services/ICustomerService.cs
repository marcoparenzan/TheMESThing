using TheMESItemsThingLib.Common;
using TheMESThingData.Entities.Mes;

namespace TheMESItemsThingLib.Services;

public interface ICustomerService
{
    Task<PagedResult<Customer>> GetListAsync(Guid? tenantId, int page, int pageSize, CancellationToken ct = default);
    Task<Customer?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<Customer> CreateAsync(Customer body, CancellationToken ct = default);
    Task<Customer?> UpdateAsync(Guid id, Customer body, CancellationToken ct = default);
    Task<bool> DeleteAsync(Guid id, CancellationToken ct = default);
}
