namespace TheMESThingData.Entities.Analytics;

public class OeeCalculation
{
    public long OeeCalculationId { get; set; }
    public Guid TenantId { get; set; }
    public Guid MachineId { get; set; }
    public Guid? ShiftId { get; set; }
    public Guid? ProductionOrderId { get; set; }
    public DateOnly PeriodDate { get; set; }
    public string PeriodType { get; set; } = null!;
    public long PlannedProductionTimeSeconds { get; set; }
    public long OperatingTimeSeconds { get; set; }
    public long RunTimeSeconds { get; set; }
    public decimal PlannedQuantity { get; set; }
    public decimal GoodQuantity { get; set; }
    public decimal TotalQuantity { get; set; }
    public decimal? Availability { get; set; }
    public decimal? Performance { get; set; }
    public decimal? Quality { get; set; }
    public decimal? OeeValue { get; set; }
    public long? MeanTimeBetweenFailureSeconds { get; set; }
    public long? MeanTimeToRepairSeconds { get; set; }
    public long ScheduledDowntimeSeconds { get; set; }
    public long UnscheduledDowntimeSeconds { get; set; }
    public DateTime CalculatedAtUtc { get; set; }
}
