using Microsoft.EntityFrameworkCore;
using TheMESItemsThingLib.Common;
using TheMESItemsThingLib.Services;
using TheMESThingData;
using TheMESThingData.Entities.Mes;

namespace TheMESThingLib.Services;

public sealed class WorkOrderService(TheMESThingDbContext db) : IWorkOrderService
{
    public async Task<PagedResult<WorkOrder>> GetListAsync(Guid? tenantId, string? status, int page, int pageSize, CancellationToken ct = default)
    {
        var q = db.WorkOrders.AsNoTracking();
        if (tenantId.HasValue) q = q.Where(w => w.TenantId == tenantId.Value);
        if (!string.IsNullOrEmpty(status)) q = q.Where(w => w.Status == status);
        var total = await q.CountAsync(ct);
        var items = await q.OrderByDescending(w => w.CreatedAtUtc)
            .Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(ct);
        return new PagedResult<WorkOrder>(items, page, pageSize, total);
    }

    public Task<WorkOrder?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        db.WorkOrders.AsNoTracking().FirstOrDefaultAsync(w => w.WorkOrderId == id, ct);

    public async Task<WorkOrder> CreateAsync(WorkOrder body, CancellationToken ct = default)
    {
        body.WorkOrderId = Guid.Empty;
        db.WorkOrders.Add(body);
        await db.SaveChangesAsync(ct);
        return body;
    }

    public async Task<WorkOrder?> UpdateAsync(Guid id, WorkOrder body, CancellationToken ct = default)
    {
        var e = await db.WorkOrders.FindAsync([id], ct);
        if (e is null) return null;
        e.WorkOrderNumber = body.WorkOrderNumber;
        e.Description = body.Description;
        e.Status = body.Status;
        e.Priority = body.Priority;
        e.DueDate = body.DueDate;
        e.PlannedStartDate = body.PlannedStartDate;
        e.ActualStartDate = body.ActualStartDate;
        e.ActualEndDate = body.ActualEndDate;
        e.TotalQuantity = body.TotalQuantity;
        e.CompletedQuantity = body.CompletedQuantity;
        e.RejectedQuantity = body.RejectedQuantity;
        e.ExternalMicrosoft365Id = body.ExternalMicrosoft365Id;
        e.TeamsChannelId = body.TeamsChannelId;
        e.UpdatedAtUtc = DateTime.UtcNow;
        e.UpdatedByUserId = body.UpdatedByUserId;
        await db.SaveChangesAsync(ct);
        return e;
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var e = await db.WorkOrders.FindAsync([id], ct);
        if (e is null) return false;
        db.WorkOrders.Remove(e);
        await db.SaveChangesAsync(ct);
        return true;
    }
}
