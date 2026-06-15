namespace TheItemsThingLib;

public sealed class CalendarEvent
{
    public required string Id { get; init; }
    public string? Subject { get; init; }
    public string? BodyContent { get; init; }
    public DateTimeOffset? Start { get; init; }
    public DateTimeOffset? End { get; init; }
    public string? Location { get; init; }
    public bool IsAllDay { get; init; }
    public bool IsOnlineMeeting { get; init; }
    public string? OnlineMeetingUrl { get; init; }
    public string? OrganizerEmail { get; init; }
    public IReadOnlyList<string> AttendeeEmails { get; init; } = [];
}
