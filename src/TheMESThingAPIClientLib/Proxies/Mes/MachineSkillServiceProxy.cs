using TheMESItemsThingLib.Common;
using TheMESItemsThingLib.Services;
using TheMESThingData.Entities.Mes;

namespace TheMESThingAPIClientLib.Proxies.Mes;

public sealed class MachineSkillServiceProxy(HttpClient http) : ProxyBase(http), IMachineSkillService
{
    public Task<PagedResult<MachineSkill>> GetListAsync(Guid? machineId, int page, int pageSize, CancellationToken ct = default) =>
        GetRequiredAsync<PagedResult<MachineSkill>>(
            $"api/mes/machine-skills?page={page}&pageSize={pageSize}{(machineId.HasValue ? $"&machineId={machineId}" : "")}", ct);

    public Task<MachineSkill?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        GetNullableAsync<MachineSkill>($"api/mes/machine-skills/{id}", ct);

    public Task<MachineSkill> CreateAsync(MachineSkill body, CancellationToken ct = default) =>
        PostAsync<MachineSkill, MachineSkill>("api/mes/machine-skills", body, ct);

    public Task<bool> DeleteAsync(Guid id, CancellationToken ct = default) =>
        DeleteBoolAsync($"api/mes/machine-skills/{id}", ct);
}
