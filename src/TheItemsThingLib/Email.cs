namespace TheItemsThingLib;

public sealed class Email
{
    public required string Id { get; init; }
    public string? Subject { get; init; }
    public string? BodyContent { get; init; }
    public string? FromEmail { get; init; }
    public string? FromName { get; init; }
    public IReadOnlyList<string> ToEmails { get; init; } = [];
    public IReadOnlyList<string> CcEmails { get; init; } = [];
    public DateTimeOffset? ReceivedDateTime { get; init; }
    public bool IsRead { get; init; }
    public bool HasAttachments { get; init; }
}
