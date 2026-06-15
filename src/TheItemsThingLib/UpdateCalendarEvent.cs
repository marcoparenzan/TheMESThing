namespace TheItemsThingLib;

public sealed class UpdateCalendarEvent
{
    public string? Subject { get; init; }
    public DateTimeOffset? Start { get; init; }
    public DateTimeOffset? End { get; init; }
    public string? BodyContent { get; init; }
    public string? Location { get; init; }
    public bool? IsAllDay { get; init; }
    public bool? IsOnlineMeeting { get; init; }
    public IReadOnlyList<string>? AttendeeEmails { get; init; }
}
