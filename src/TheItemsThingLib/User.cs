namespace TheItemsThingLib;

public sealed class User
{
    public required string Id { get; init; }
    public string? DisplayName { get; init; }
    public string? GivenName { get; init; }
    public string? Surname { get; init; }
    public string? Mail { get; init; }
    public string? UserPrincipalName { get; init; }
    public string? JobTitle { get; init; }
    public string? Department { get; init; }
    public string? MobilePhone { get; init; }
    public string? OfficeLocation { get; init; }
    public bool AccountEnabled { get; init; }
}
