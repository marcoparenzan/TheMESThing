using TheMESItemsThingLib.Common;
using TheMESItemsThingLib.Services;
using TheMESThingData.Entities.Mes;

namespace TheMESThingAPIClientLib.Proxies.Mes;

public sealed class OperatorSkillServiceProxy(HttpClient http) : ProxyBase(http), IOperatorSkillService
{
    public Task<PagedResult<OperatorSkill>> GetListAsync(Guid? operatorId, int page, int pageSize, CancellationToken ct = default) =>
        GetRequiredAsync<PagedResult<OperatorSkill>>(
            $"api/mes/operator-skills?page={page}&pageSize={pageSize}{(operatorId.HasValue ? $"&operatorId={operatorId}" : "")}", ct);

    public Task<OperatorSkill?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        GetNullableAsync<OperatorSkill>($"api/mes/operator-skills/{id}", ct);

    public Task<OperatorSkill> CreateAsync(OperatorSkill body, CancellationToken ct = default) =>
        PostAsync<OperatorSkill, OperatorSkill>("api/mes/operator-skills", body, ct);

    public Task<OperatorSkill?> UpdateAsync(Guid id, OperatorSkill body, CancellationToken ct = default) =>
        PutNullableAsync($"api/mes/operator-skills/{id}", body, ct);

    public Task<bool> DeleteAsync(Guid id, CancellationToken ct = default) =>
        DeleteBoolAsync($"api/mes/operator-skills/{id}", ct);
}
