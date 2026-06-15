using TheMESItemsThingLib.Common;
using TheMESItemsThingLib.Services;
using TheMESThingData.Entities.Mes;

namespace TheMESThingAPIClientLib.Proxies.Mes;

public sealed class SkillServiceProxy(HttpClient http) : ProxyBase(http), ISkillService
{
    public Task<PagedResult<Skill>> GetListAsync(Guid? tenantId, int page, int pageSize, CancellationToken ct = default) =>
        GetRequiredAsync<PagedResult<Skill>>(
            $"api/mes/skills?page={page}&pageSize={pageSize}{(tenantId.HasValue ? $"&tenantId={tenantId}" : "")}", ct);

    public Task<Skill?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        GetNullableAsync<Skill>($"api/mes/skills/{id}", ct);

    public Task<Skill> CreateAsync(Skill body, CancellationToken ct = default) =>
        PostAsync<Skill, Skill>("api/mes/skills", body, ct);

    public Task<Skill?> UpdateAsync(Guid id, Skill body, CancellationToken ct = default) =>
        PutNullableAsync($"api/mes/skills/{id}", body, ct);

    public Task<bool> DeleteAsync(Guid id, CancellationToken ct = default) =>
        DeleteBoolAsync($"api/mes/skills/{id}", ct);
}
