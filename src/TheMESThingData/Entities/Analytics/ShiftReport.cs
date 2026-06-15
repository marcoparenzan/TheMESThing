namespace TheMESThingData.Entities.Analytics;

public class ShiftReport
{
    public long ShiftReportId { get; set; }
    public Guid TenantId { get; set; }
    public Guid ShiftId { get; set; }
    public Guid MachineId { get; set; }
    public Guid? OperatorId { get; set; }
    public DateOnly ShiftDate { get; set; }
    public DateTime ShiftStartAtUtc { get; set; }
    public DateTime? ShiftEndAtUtc { get; set; }
    public long PlannedProductionTimeSeconds { get; set; }
    public long TotalRunTimeSeconds { get; set; }
    public long TotalDownTimeSeconds { get; set; }
    public long TotalIdleTimeSeconds { get; set; }
    public decimal GoodQuantity { get; set; }
    public decimal ScrapQuantity { get; set; }
    public int TotalCycles { get; set; }
    public decimal? OeeValue { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAtUtc { get; set; }
}
