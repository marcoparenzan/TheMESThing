using TheMESItemsThingLib.Common;
using TheMESItemsThingLib.Services;
using TheMESThingData.Entities.Mes;

namespace TheMESThingAPIClientLib.Proxies.Mes;

public sealed class ProductServiceProxy(HttpClient http) : ProxyBase(http), IProductService
{
    public Task<PagedResult<Product>> GetListAsync(Guid? tenantId, int page, int pageSize, CancellationToken ct = default) =>
        GetRequiredAsync<PagedResult<Product>>(
            $"api/mes/products?page={page}&pageSize={pageSize}{(tenantId.HasValue ? $"&tenantId={tenantId}" : "")}", ct);

    public Task<Product?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        GetNullableAsync<Product>($"api/mes/products/{id}", ct);

    public Task<Product> CreateAsync(Product body, CancellationToken ct = default) =>
        PostAsync<Product, Product>("api/mes/products", body, ct);

    public Task<Product?> UpdateAsync(Guid id, Product body, CancellationToken ct = default) =>
        PutNullableAsync($"api/mes/products/{id}", body, ct);

    public Task<bool> DeleteAsync(Guid id, CancellationToken ct = default) =>
        DeleteBoolAsync($"api/mes/products/{id}", ct);
}
