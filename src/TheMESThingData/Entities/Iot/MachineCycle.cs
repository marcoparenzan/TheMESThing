namespace TheMESThingData.Entities.Iot;

public class MachineCycle
{
    public long MachineCycleId { get; set; }
    public Guid TenantId { get; set; }
    public Guid MachineId { get; set; }
    public Guid ProductionOrderId { get; set; }
    public Guid? ShiftId { get; set; }
    public Guid? OperatorId { get; set; }
    public DateTime CycleStartAtUtc { get; set; }
    public DateTime CycleEndAtUtc { get; set; }
    // CycleDurationSeconds is a PERSISTED computed column — mapped as read-only
    public int? CycleDurationSeconds { get; set; }
    public decimal GoodQuantity { get; set; }
    public decimal ScrapQuantity { get; set; }
    public bool IsRejected { get; set; }
    public string? ScrapReasonCode { get; set; }
    public string? IoTMessageId { get; set; }
    public DateOnly CycleDateUtc { get; set; }
}
