using TheMESItemsThingLib.Common;
using TheMESItemsThingLib.Services;
using TheMESThingData.Entities.Mes;

namespace TheMESThingAPIClientLib.Proxies.Mes;

public sealed class CustomerServiceProxy(HttpClient http) : ProxyBase(http), ICustomerService
{
    public Task<PagedResult<Customer>> GetListAsync(Guid? tenantId, int page, int pageSize, CancellationToken ct = default) =>
        GetRequiredAsync<PagedResult<Customer>>(
            $"api/mes/customers?page={page}&pageSize={pageSize}{(tenantId.HasValue ? $"&tenantId={tenantId}" : "")}", ct);

    public Task<Customer?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        GetNullableAsync<Customer>($"api/mes/customers/{id}", ct);

    public Task<Customer> CreateAsync(Customer body, CancellationToken ct = default) =>
        PostAsync<Customer, Customer>("api/mes/customers", body, ct);

    public Task<Customer?> UpdateAsync(Guid id, Customer body, CancellationToken ct = default) =>
        PutNullableAsync($"api/mes/customers/{id}", body, ct);

    public Task<bool> DeleteAsync(Guid id, CancellationToken ct = default) =>
        DeleteBoolAsync($"api/mes/customers/{id}", ct);
}
