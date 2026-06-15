namespace The365ThingLib;

public sealed class M365Config
{
    public required string TenantId { get; init; }
    public required string ClientId { get; init; }
    public required string ClientSecret { get; init; }
}
