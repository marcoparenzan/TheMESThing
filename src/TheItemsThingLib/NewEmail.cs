namespace TheItemsThingLib;

public sealed class NewEmail
{
    public required string Subject { get; init; }
    public required string BodyContent { get; init; }
    public required IReadOnlyList<string> ToEmails { get; init; }
    public IReadOnlyList<string> CcEmails { get; init; } = [];
    public IReadOnlyList<string> BccEmails { get; init; } = [];
    public bool IsHtml { get; init; } = true;
}
