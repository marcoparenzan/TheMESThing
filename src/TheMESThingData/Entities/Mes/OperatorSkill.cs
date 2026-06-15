namespace TheMESThingData.Entities.Mes;

public class OperatorSkill
{
    public Guid OperatorSkillId { get; set; }
    public Guid OperatorId { get; set; }
    public Guid SkillId { get; set; }
    public byte ProficiencyLevel { get; set; }
    public DateTime? CertifiedAtUtc { get; set; }
    public DateTime? ExpiresAtUtc { get; set; }

    public Operator Operator { get; set; } = null!;
    public Skill Skill { get; set; } = null!;
}
