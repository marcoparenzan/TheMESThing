using TheMESItemsThingLib.Common;
using TheMESThingData.Entities.Mes;

namespace TheMESItemsThingLib.Services;

public interface IShiftService
{
    Task<PagedResult<Shift>> GetListAsync(Guid? tenantId, int page, int pageSize, CancellationToken ct = default);
    Task<Shift?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<Shift> CreateAsync(Shift body, CancellationToken ct = default);
    Task<Shift?> UpdateAsync(Guid id, Shift body, CancellationToken ct = default);
    Task<bool> DeleteAsync(Guid id, CancellationToken ct = default);
}
