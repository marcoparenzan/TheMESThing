namespace TheMESThingData.Entities.Mes;

public class MachineSkill
{
    public Guid MachineSkillId { get; set; }
    public Guid MachineId { get; set; }
    public Guid SkillId { get; set; }
    public bool IsRequired { get; set; }

    public Machine Machine { get; set; } = null!;
    public Skill Skill { get; set; } = null!;
}
