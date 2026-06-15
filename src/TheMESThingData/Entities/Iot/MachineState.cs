namespace TheMESThingData.Entities.Iot;

public class MachineState
{
    public int MachineStateId { get; set; }
    public string StateCode { get; set; } = null!;
    public string StateName { get; set; } = null!;
    public string StateCategory { get; set; } = null!;
    public bool IsProductiveTime { get; set; }
    public bool IsPlannedStop { get; set; }
    public string? ColorHex { get; set; }
    public int SortOrder { get; set; }

    public ICollection<MachineStatusHistory> MachineStatusHistories { get; set; } = [];
}
