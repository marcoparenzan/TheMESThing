namespace TheMESThingData.Entities.Iot;

public class MachineEvent
{
    public long MachineEventId { get; set; }
    public Guid TenantId { get; set; }
    public Guid MachineId { get; set; }
    public string EventType { get; set; } = null!;
    public string EventCode { get; set; } = null!;
    public string Severity { get; set; } = null!;
    public string? EventMessage { get; set; }
    public string? EventPayloadJson { get; set; }
    public DateTime OccurredAtUtc { get; set; }
    public DateTime? AcknowledgedAtUtc { get; set; }
    public Guid? AcknowledgedByUserId { get; set; }
    public Guid? ProductionOrderId { get; set; }
    public string? IoTMessageId { get; set; }
    public DateOnly EventDateUtc { get; set; }
}
