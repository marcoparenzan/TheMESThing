using TheMESItemsThingLib.Common;
using TheMESItemsThingLib.Services;
using TheMESThingData.Entities.Mes;

namespace TheMESThingAPIClientLib.Proxies.Mes;

public sealed class DepartmentServiceProxy(HttpClient http) : ProxyBase(http), IDepartmentService
{
    public Task<PagedResult<Department>> GetListAsync(Guid? tenantId, int page, int pageSize, CancellationToken ct = default) =>
        GetRequiredAsync<PagedResult<Department>>(
            $"api/mes/departments?page={page}&pageSize={pageSize}{(tenantId.HasValue ? $"&tenantId={tenantId}" : "")}", ct);

    public Task<Department?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        GetNullableAsync<Department>($"api/mes/departments/{id}", ct);

    public Task<Department> CreateAsync(Department body, CancellationToken ct = default) =>
        PostAsync<Department, Department>("api/mes/departments", body, ct);

    public Task<Department?> UpdateAsync(Guid id, Department body, CancellationToken ct = default) =>
        PutNullableAsync($"api/mes/departments/{id}", body, ct);

    public Task<bool> DeleteAsync(Guid id, CancellationToken ct = default) =>
        DeleteBoolAsync($"api/mes/departments/{id}", ct);
}
