namespace TheItemsThingLib;

public sealed class MailFolder
{
    public required string Id { get; init; }
    public required string DisplayName { get; init; }
    public int UnreadItemCount { get; init; }
    public int TotalItemCount { get; init; }
}
