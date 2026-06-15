using Microsoft.EntityFrameworkCore;
using TheMESItemsThingLib.Common;
using TheMESItemsThingLib.Services;
using TheMESThingData;
using TheMESThingData.Entities.Mes;

namespace TheMESThingLib.Services;

public sealed class MachineService(TheMESThingDbContext db) : IMachineService
{
    public async Task<PagedResult<Machine>> GetListAsync(Guid? tenantId, Guid? departmentId, int page, int pageSize, CancellationToken ct = default)
    {
        var q = db.Machines.AsNoTracking();
        if (tenantId.HasValue) q = q.Where(m => m.TenantId == tenantId.Value);
        if (departmentId.HasValue) q = q.Where(m => m.DepartmentId == departmentId.Value);
        var total = await q.CountAsync(ct);
        var items = await q.OrderBy(m => m.MachineName)
            .Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(ct);
        return new PagedResult<Machine>(items, page, pageSize, total);
    }

    public Task<Machine?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        db.Machines.AsNoTracking().FirstOrDefaultAsync(m => m.MachineId == id, ct);

    public async Task<Machine> CreateAsync(Machine body, CancellationToken ct = default)
    {
        body.MachineId = Guid.Empty;
        db.Machines.Add(body);
        await db.SaveChangesAsync(ct);
        return body;
    }

    public async Task<Machine?> UpdateAsync(Guid id, Machine body, CancellationToken ct = default)
    {
        var e = await db.Machines.FindAsync([id], ct);
        if (e is null) return null;
        e.MachineCode = body.MachineCode;
        e.MachineName = body.MachineName;
        e.MachineType = body.MachineType;
        e.Manufacturer = body.Manufacturer;
        e.Model = body.Model;
        e.SerialNumber = body.SerialNumber;
        e.InstallationDate = body.InstallationDate;
        e.NominalCycleTimeSeconds = body.NominalCycleTimeSeconds;
        e.NominalCapacityPerHour = body.NominalCapacityPerHour;
        e.IoTDeviceId = body.IoTDeviceId;
        e.IoTHubConnectionState = body.IoTHubConnectionState;
        e.CurrentStatus = body.CurrentStatus;
        e.ExternalMicrosoft365Id = body.ExternalMicrosoft365Id;
        e.IsActive = body.IsActive;
        e.UpdatedAtUtc = DateTime.UtcNow;
        await db.SaveChangesAsync(ct);
        return e;
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var e = await db.Machines.FindAsync([id], ct);
        if (e is null) return false;
        db.Machines.Remove(e);
        await db.SaveChangesAsync(ct);
        return true;
    }
}
