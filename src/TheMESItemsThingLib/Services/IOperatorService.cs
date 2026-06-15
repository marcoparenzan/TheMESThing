using TheMESItemsThingLib.Common;
using TheMESThingData.Entities.Mes;

namespace TheMESItemsThingLib.Services;

public interface IOperatorService
{
    Task<PagedResult<Operator>> GetListAsync(Guid? tenantId, Guid? departmentId, int page, int pageSize, CancellationToken ct = default);
    Task<Operator?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<Operator> CreateAsync(Operator body, CancellationToken ct = default);
    Task<Operator?> UpdateAsync(Guid id, Operator body, CancellationToken ct = default);
    Task<bool> DeleteAsync(Guid id, CancellationToken ct = default);
}
