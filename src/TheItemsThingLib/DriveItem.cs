namespace TheItemsThingLib;

public sealed class DriveItem
{
    public required string Id { get; init; }
    public required string Name { get; init; }
    public string? DisplayPath { get; init; }
    public bool IsFolder { get; init; }
    public long? Size { get; init; }
    public DateTimeOffset? CreatedDateTime { get; init; }
    public DateTimeOffset? ModifiedDateTime { get; init; }
    public string? WebUrl { get; init; }
    public string? ParentId { get; init; }
    public string? MimeType { get; init; }
}
