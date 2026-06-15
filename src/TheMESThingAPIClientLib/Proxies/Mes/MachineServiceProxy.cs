using TheMESItemsThingLib.Common;
using TheMESItemsThingLib.Services;
using TheMESThingData.Entities.Mes;

namespace TheMESThingAPIClientLib.Proxies.Mes;

public sealed class MachineServiceProxy(HttpClient http) : ProxyBase(http), IMachineService
{
    public Task<PagedResult<Machine>> GetListAsync(Guid? tenantId, Guid? departmentId, int page, int pageSize, CancellationToken ct = default) =>
        GetRequiredAsync<PagedResult<Machine>>(
            $"api/mes/machines?page={page}&pageSize={pageSize}" +
            $"{(tenantId.HasValue ? $"&tenantId={tenantId}" : "")}" +
            $"{(departmentId.HasValue ? $"&departmentId={departmentId}" : "")}", ct);

    public Task<Machine?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        GetNullableAsync<Machine>($"api/mes/machines/{id}", ct);

    public Task<Machine> CreateAsync(Machine body, CancellationToken ct = default) =>
        PostAsync<Machine, Machine>("api/mes/machines", body, ct);

    public Task<Machine?> UpdateAsync(Guid id, Machine body, CancellationToken ct = default) =>
        PutNullableAsync($"api/mes/machines/{id}", body, ct);

    public Task<bool> DeleteAsync(Guid id, CancellationToken ct = default) =>
        DeleteBoolAsync($"api/mes/machines/{id}", ct);
}
