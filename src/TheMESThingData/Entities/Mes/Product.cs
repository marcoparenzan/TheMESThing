namespace TheMESThingData.Entities.Mes;

public class Product
{
    public Guid ProductId { get; set; }
    public Guid TenantId { get; set; }
    public string ProductCode { get; set; } = null!;
    public string ProductName { get; set; } = null!;
    public string UnitOfMeasure { get; set; } = null!;
    public decimal? CycleTimeSeconds { get; set; }
    public decimal? SetupTimeSeconds { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public DateTime UpdatedAtUtc { get; set; }

    public ICollection<ProductionOrder> ProductionOrders { get; set; } = [];
}
