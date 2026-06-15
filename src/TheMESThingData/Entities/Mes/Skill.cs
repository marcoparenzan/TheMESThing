namespace TheMESThingData.Entities.Mes;

public class Skill
{
    public Guid SkillId { get; set; }
    public Guid TenantId { get; set; }
    public string SkillCode { get; set; } = null!;
    public string SkillName { get; set; } = null!;
    public string? Category { get; set; }
    public string? Description { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAtUtc { get; set; }

    public ICollection<MachineSkill> MachineSkills { get; set; } = [];
    public ICollection<OperatorSkill> OperatorSkills { get; set; } = [];
}
