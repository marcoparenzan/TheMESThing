using TheMESItemsThingLib.Common;
using TheMESItemsThingLib.Services;
using TheMESThingData.Entities.Mes;

namespace TheMESThingAPIClientLib.Proxies.Mes;

public sealed class WorkOrderServiceProxy(HttpClient http) : ProxyBase(http), IWorkOrderService
{
    public Task<PagedResult<WorkOrder>> GetListAsync(Guid? tenantId, string? status, int page, int pageSize, CancellationToken ct = default) =>
        GetRequiredAsync<PagedResult<WorkOrder>>(
            $"api/mes/work-orders?page={page}&pageSize={pageSize}" +
            $"{(tenantId.HasValue ? $"&tenantId={tenantId}" : "")}" +
            $"{(status != null ? $"&status={status}" : "")}", ct);

    public Task<WorkOrder?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        GetNullableAsync<WorkOrder>($"api/mes/work-orders/{id}", ct);

    public Task<WorkOrder> CreateAsync(WorkOrder body, CancellationToken ct = default) =>
        PostAsync<WorkOrder, WorkOrder>("api/mes/work-orders", body, ct);

    public Task<WorkOrder?> UpdateAsync(Guid id, WorkOrder body, CancellationToken ct = default) =>
        PutNullableAsync($"api/mes/work-orders/{id}", body, ct);

    public Task<bool> DeleteAsync(Guid id, CancellationToken ct = default) =>
        DeleteBoolAsync($"api/mes/work-orders/{id}", ct);
}
