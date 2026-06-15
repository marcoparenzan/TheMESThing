namespace TheMESThingData.Entities.Mes;

public class ProductionLine
{
    public Guid ProductionLineId { get; set; }
    public Guid TenantId { get; set; }
    public Guid DepartmentId { get; set; }
    public string LineCode { get; set; } = null!;
    public string LineName { get; set; } = null!;
    public string LineType { get; set; } = null!;
    public decimal? NominalCapacity { get; set; }
    public string? CapacityUnit { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public DateTime UpdatedAtUtc { get; set; }

    public Department Department { get; set; } = null!;
    public ICollection<Machine> Machines { get; set; } = [];
    public ICollection<ProductionOrder> ProductionOrders { get; set; } = [];
}
