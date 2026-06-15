namespace TheMESThingData.Entities.Iot;

public class MachineTelemetry
{
    public long MachineTelemetryId { get; set; }
    public Guid TenantId { get; set; }
    public Guid MachineId { get; set; }
    public string MetricName { get; set; } = null!;
    public double MetricValue { get; set; }
    public string? MetricUnit { get; set; }
    public DateTime RecordedAtUtc { get; set; }
    public Guid? ProductionOrderId { get; set; }
    public string? IoTMessageId { get; set; }
    public DateOnly TelemetryDateUtc { get; set; }
}
