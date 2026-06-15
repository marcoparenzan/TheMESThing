namespace TheMESThingData.Entities.Mes;

public class Operator
{
    public Guid OperatorId { get; set; }
    public Guid TenantId { get; set; }
    public Guid? AppUserId { get; set; }
    public Guid? DepartmentId { get; set; }
    public string OperatorCode { get; set; } = null!;
    public string FirstName { get; set; } = null!;
    public string LastName { get; set; } = null!;
    public string? BadgeNumber { get; set; }
    public string? ExternalMicrosoft365Id { get; set; }
    public bool IsActive { get; set; }
    public DateOnly? HireDate { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public DateTime UpdatedAtUtc { get; set; }

    public Department? Department { get; set; }
    public ICollection<OperatorSkill> OperatorSkills { get; set; } = [];
}
