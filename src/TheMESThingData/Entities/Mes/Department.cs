namespace TheMESThingData.Entities.Mes;

public class Department
{
    public Guid DepartmentId { get; set; }
    public Guid TenantId { get; set; }
    public string DepartmentCode { get; set; } = null!;
    public string DepartmentName { get; set; } = null!;
    public Guid? ParentDepartmentId { get; set; }
    public Guid? ManagerUserId { get; set; }
    public string? CostCenter { get; set; }
    public string? ExternalMicrosoft365Id { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public DateTime UpdatedAtUtc { get; set; }

    public Department? ParentDepartment { get; set; }
    public ICollection<Department> ChildDepartments { get; set; } = [];
    public ICollection<Machine> Machines { get; set; } = [];
    public ICollection<ProductionLine> ProductionLines { get; set; } = [];
    public ICollection<Operator> Operators { get; set; } = [];
}
