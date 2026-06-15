using TheMESItemsThingLib.Common;
using TheMESItemsThingLib.Services;
using TheMESThingData.Entities.Mes;

namespace TheMESThingAPIClientLib.Proxies.Mes;

public sealed class OperatorServiceProxy(HttpClient http) : ProxyBase(http), IOperatorService
{
    public Task<PagedResult<Operator>> GetListAsync(Guid? tenantId, Guid? departmentId, int page, int pageSize, CancellationToken ct = default) =>
        GetRequiredAsync<PagedResult<Operator>>(
            $"api/mes/operators?page={page}&pageSize={pageSize}" +
            $"{(tenantId.HasValue ? $"&tenantId={tenantId}" : "")}" +
            $"{(departmentId.HasValue ? $"&departmentId={departmentId}" : "")}", ct);

    public Task<Operator?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        GetNullableAsync<Operator>($"api/mes/operators/{id}", ct);

    public Task<Operator> CreateAsync(Operator body, CancellationToken ct = default) =>
        PostAsync<Operator, Operator>("api/mes/operators", body, ct);

    public Task<Operator?> UpdateAsync(Guid id, Operator body, CancellationToken ct = default) =>
        PutNullableAsync($"api/mes/operators/{id}", body, ct);

    public Task<bool> DeleteAsync(Guid id, CancellationToken ct = default) =>
        DeleteBoolAsync($"api/mes/operators/{id}", ct);
}
