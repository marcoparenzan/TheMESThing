using TheMESItemsThingLib.Common;
using TheMESItemsThingLib.Services;
using TheMESThingData.Entities.Mes;

namespace TheMESThingAPIClientLib.Proxies.Mes;

public sealed class ProductionOrderServiceProxy(HttpClient http) : ProxyBase(http), IProductionOrderService
{
    public Task<PagedResult<ProductionOrder>> GetListAsync(Guid? tenantId, Guid? machineId, string? status, int page, int pageSize, CancellationToken ct = default) =>
        GetRequiredAsync<PagedResult<ProductionOrder>>(
            $"api/mes/production-orders?page={page}&pageSize={pageSize}" +
            $"{(tenantId.HasValue ? $"&tenantId={tenantId}" : "")}" +
            $"{(machineId.HasValue ? $"&machineId={machineId}" : "")}" +
            $"{(status != null ? $"&status={status}" : "")}", ct);

    public Task<ProductionOrder?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        GetNullableAsync<ProductionOrder>($"api/mes/production-orders/{id}", ct);

    public Task<ProductionOrder> CreateAsync(ProductionOrder body, CancellationToken ct = default) =>
        PostAsync<ProductionOrder, ProductionOrder>("api/mes/production-orders", body, ct);

    public Task<ProductionOrder?> UpdateAsync(Guid id, ProductionOrder body, CancellationToken ct = default) =>
        PutNullableAsync($"api/mes/production-orders/{id}", body, ct);

    public Task<bool> DeleteAsync(Guid id, CancellationToken ct = default) =>
        DeleteBoolAsync($"api/mes/production-orders/{id}", ct);
}
