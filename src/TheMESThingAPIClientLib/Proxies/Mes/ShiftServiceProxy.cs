using TheMESItemsThingLib.Common;
using TheMESItemsThingLib.Services;
using TheMESThingData.Entities.Mes;

namespace TheMESThingAPIClientLib.Proxies.Mes;

public sealed class ShiftServiceProxy(HttpClient http) : ProxyBase(http), IShiftService
{
    public Task<PagedResult<Shift>> GetListAsync(Guid? tenantId, int page, int pageSize, CancellationToken ct = default) =>
        GetRequiredAsync<PagedResult<Shift>>(
            $"api/mes/shifts?page={page}&pageSize={pageSize}{(tenantId.HasValue ? $"&tenantId={tenantId}" : "")}", ct);

    public Task<Shift?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        GetNullableAsync<Shift>($"api/mes/shifts/{id}", ct);

    public Task<Shift> CreateAsync(Shift body, CancellationToken ct = default) =>
        PostAsync<Shift, Shift>("api/mes/shifts", body, ct);

    public Task<Shift?> UpdateAsync(Guid id, Shift body, CancellationToken ct = default) =>
        PutNullableAsync($"api/mes/shifts/{id}", body, ct);

    public Task<bool> DeleteAsync(Guid id, CancellationToken ct = default) =>
        DeleteBoolAsync($"api/mes/shifts/{id}", ct);
}
