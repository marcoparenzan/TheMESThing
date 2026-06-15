namespace TheMESThingData.Entities.Mes;

public class WorkOrder
{
    public Guid WorkOrderId { get; set; }
    public Guid TenantId { get; set; }
    public Guid CustomerId { get; set; }
    public string WorkOrderNumber { get; set; } = null!;
    public string? Description { get; set; }
    public string Status { get; set; } = "Draft";
    public byte Priority { get; set; }
    public DateOnly? DueDate { get; set; }
    public DateOnly? PlannedStartDate { get; set; }
    public DateOnly? ActualStartDate { get; set; }
    public DateOnly? ActualEndDate { get; set; }
    public decimal TotalQuantity { get; set; }
    public decimal CompletedQuantity { get; set; }
    public decimal RejectedQuantity { get; set; }
    public string? ExternalMicrosoft365Id { get; set; }
    public string? TeamsChannelId { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public DateTime UpdatedAtUtc { get; set; }
    public Guid? CreatedByUserId { get; set; }
    public Guid? UpdatedByUserId { get; set; }

    public Customer Customer { get; set; } = null!;
    public ICollection<ProductionOrder> ProductionOrders { get; set; } = [];
}
