namespace TheMESThingData.Entities.Mes;

public class Shift
{
    public Guid ShiftId { get; set; }
    public Guid TenantId { get; set; }
    public string ShiftCode { get; set; } = null!;
    public string ShiftName { get; set; } = null!;
    public TimeOnly StartTime { get; set; }
    public TimeOnly EndTime { get; set; }
    public int BreakMinutes { get; set; }
    public bool IsNightShift { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAtUtc { get; set; }

    public ICollection<ProductionOrder> ProductionOrders { get; set; } = [];
}
