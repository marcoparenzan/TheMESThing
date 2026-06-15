using TheMESItemsThingLib.Common;
using TheMESItemsThingLib.Services;
using TheMESThingData.Entities.Mes;

namespace TheMESThingAPIClientLib.Proxies.Mes;

public sealed class ProductionLineServiceProxy(HttpClient http) : ProxyBase(http), IProductionLineService
{
    public Task<PagedResult<ProductionLine>> GetListAsync(Guid? tenantId, int page, int pageSize, CancellationToken ct = default) =>
        GetRequiredAsync<PagedResult<ProductionLine>>(
            $"api/mes/production-lines?page={page}&pageSize={pageSize}{(tenantId.HasValue ? $"&tenantId={tenantId}" : "")}", ct);

    public Task<ProductionLine?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        GetNullableAsync<ProductionLine>($"api/mes/production-lines/{id}", ct);

    public Task<ProductionLine> CreateAsync(ProductionLine body, CancellationToken ct = default) =>
        PostAsync<ProductionLine, ProductionLine>("api/mes/production-lines", body, ct);

    public Task<ProductionLine?> UpdateAsync(Guid id, ProductionLine body, CancellationToken ct = default) =>
        PutNullableAsync($"api/mes/production-lines/{id}", body, ct);

    public Task<bool> DeleteAsync(Guid id, CancellationToken ct = default) =>
        DeleteBoolAsync($"api/mes/production-lines/{id}", ct);
}
