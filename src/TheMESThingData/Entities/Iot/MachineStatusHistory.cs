namespace TheMESThingData.Entities.Iot;

public class MachineStatusHistory
{
    public long MachineStatusHistoryId { get; set; }
    public Guid TenantId { get; set; }
    public Guid MachineId { get; set; }
    public int MachineStateId { get; set; }
    public DateTime StartedAtUtc { get; set; }
    public DateTime? EndedAtUtc { get; set; }
    // DurationSeconds is a non-deterministic computed column — not mapped
    public Guid? ProductionOrderId { get; set; }
    public Guid? ShiftId { get; set; }
    public Guid? OperatorId { get; set; }
    public string? ReasonCode { get; set; }
    public string? Notes { get; set; }
    public string? IoTMessageId { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public DateOnly EventDateUtc { get; set; }

    public MachineState MachineState { get; set; } = null!;
}
