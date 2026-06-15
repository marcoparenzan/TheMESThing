namespace TheMESThingData.Entities.Analytics;

public class KpiDefinition
{
    public Guid KpiDefinitionId { get; set; }
    public string KpiCode { get; set; } = null!;
    public string KpiName { get; set; } = null!;
    public string KpiGroup { get; set; } = null!;
    public string? IsoReference { get; set; }
    public string? Formula { get; set; }
    public string? Unit { get; set; }
    public decimal? TargetValue { get; set; }
    public decimal? WarningThreshold { get; set; }
    public decimal? CriticalThreshold { get; set; }
    public bool IsHigherBetter { get; set; }
    public string? Description { get; set; }

    public ICollection<KpiValue> KpiValues { get; set; } = [];
}
