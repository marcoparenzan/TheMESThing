namespace TheMESThingData.Entities.Mes;

public class Customer
{
    public Guid CustomerId { get; set; }
    public Guid TenantId { get; set; }
    public string CustomerCode { get; set; } = null!;
    public string CustomerName { get; set; } = null!;
    public string? ContactEmail { get; set; }
    public string? ContactPhone { get; set; }
    public string? Address { get; set; }
    public string? Country { get; set; }
    public string? ExternalMicrosoft365Id { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public DateTime UpdatedAtUtc { get; set; }
    public Guid? CreatedByUserId { get; set; }
    public Guid? UpdatedByUserId { get; set; }

    public ICollection<WorkOrder> WorkOrders { get; set; } = [];
}
