namespace TheMESThingLib.Common;

public static class WorkOrderStatuses
{
    public static readonly string[] Valid = ["Draft", "Released", "InProgress", "Completed", "Cancelled", "OnHold"];
    public static readonly string AllowedValues = string.Join(", ", Valid);

    public static bool IsValid(string? status) =>
        status is not null && Valid.Contains(status, StringComparer.Ordinal);
}
