namespace TheMESThingData.Entities.Analytics;

public class KpiValue
{
    public long KpiValueId { get; set; }
    public Guid TenantId { get; set; }
    public Guid KpiDefinitionId { get; set; }
    public Guid? MachineId { get; set; }
    public Guid? DepartmentId { get; set; }
    public Guid? WorkOrderId { get; set; }
    public Guid? ShiftId { get; set; }
    public DateOnly PeriodDate { get; set; }
    public string PeriodType { get; set; } = null!;
    public decimal KpiValueAmount { get; set; }
    public decimal? KpiTarget { get; set; }
    public decimal? VarianceFromTarget { get; set; }
    public DateTime CalculatedAtUtc { get; set; }

    public KpiDefinition KpiDefinition { get; set; } = null!;
}
