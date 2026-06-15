namespace TheItemsThingLib;

public sealed class NewUser
{
    public required string DisplayName { get; init; }
    public required string UserPrincipalName { get; init; }
    public required string MailNickname { get; init; }
    public required string Password { get; init; }
    public string? GivenName { get; init; }
    public string? Surname { get; init; }
    public string? JobTitle { get; init; }
    public string? Department { get; init; }
    public string? MobilePhone { get; init; }
    public string? OfficeLocation { get; init; }
    public bool AccountEnabled { get; init; } = true;
    public bool ForceChangePasswordNextSignIn { get; init; } = true;
}
