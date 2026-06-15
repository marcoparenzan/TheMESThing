namespace TheMESThingData.Entities.Mes;

public class ProductionOrder
{
    public Guid ProductionOrderId { get; set; }
    public Guid TenantId { get; set; }
    public Guid WorkOrderId { get; set; }
    public Guid ProductId { get; set; }
    public Guid? MachineId { get; set; }
    public Guid? ProductionLineId { get; set; }
    public int OperationSequence { get; set; }
    public string OrderNumber { get; set; } = null!;
    public string Status { get; set; } = null!;
    public decimal PlannedQuantity { get; set; }
    public decimal GoodQuantity { get; set; }
    public decimal ScrapQuantity { get; set; }
    public decimal ReworkQuantity { get; set; }
    public DateTime? PlannedStartAtUtc { get; set; }
    public DateTime? PlannedEndAtUtc { get; set; }
    public DateTime? ActualStartAtUtc { get; set; }
    public DateTime? ActualEndAtUtc { get; set; }
    public decimal? PlannedCycleTimeSeconds { get; set; }
    public decimal? ActualCycleTimeSeconds { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public DateTime UpdatedAtUtc { get; set; }

    public WorkOrder WorkOrder { get; set; } = null!;
    public Product Product { get; set; } = null!;
    public Machine? Machine { get; set; }
    public ProductionLine? ProductionLine { get; set; }
}
