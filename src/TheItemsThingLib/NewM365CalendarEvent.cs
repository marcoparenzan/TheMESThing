namespace TheItemsThingLib;

public sealed class NewM365CalendarEvent
{
    public required string Subject { get; init; }
    public required DateTimeOffset Start { get; init; }
    public required DateTimeOffset End { get; init; }
    public string? BodyContent { get; init; }
    public string? Location { get; init; }
    public bool IsAllDay { get; init; }
    public bool IsOnlineMeeting { get; init; }
    public IReadOnlyList<string> AttendeeEmails { get; init; } = [];
}
