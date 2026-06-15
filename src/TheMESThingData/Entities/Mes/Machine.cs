namespace TheMESThingData.Entities.Mes;

public class Machine
{
    public Guid MachineId { get; set; }
    public Guid TenantId { get; set; }
    public Guid DepartmentId { get; set; }
    public Guid? ProductionLineId { get; set; }
    public string MachineCode { get; set; } = null!;
    public string MachineName { get; set; } = null!;
    public string MachineType { get; set; } = null!;
    public string? Manufacturer { get; set; }
    public string? Model { get; set; }
    public string? SerialNumber { get; set; }
    public DateOnly? InstallationDate { get; set; }
    public decimal? NominalCycleTimeSeconds { get; set; }
    public decimal? NominalCapacityPerHour { get; set; }
    public string? IoTDeviceId { get; set; }
    public string? IoTHubConnectionState { get; set; }
    public string CurrentStatus { get; set; } = null!;
    public DateTime? LastStatusChangedAtUtc { get; set; }
    public string? ExternalMicrosoft365Id { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public DateTime UpdatedAtUtc { get; set; }

    public Department Department { get; set; } = null!;
    public ProductionLine? ProductionLine { get; set; }
    public ICollection<MachineSkill> MachineSkills { get; set; } = [];
    public ICollection<ProductionOrder> ProductionOrders { get; set; } = [];
}
