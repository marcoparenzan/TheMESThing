namespace TheItemsThingLib;

public interface ICalendarService
{
    Task<CalendarEvent> CreateEventAsync(string userIdOrUpn, NewM365CalendarEvent newEvent, CancellationToken ct = default);
    Task DeleteEventAsync(string userIdOrUpn, string eventId, CancellationToken ct = default);
    Task<CalendarEvent?> GetEventAsync(string userIdOrUpn, string eventId, CancellationToken ct = default);
    Task<IReadOnlyList<CalendarEvent>> ListEventsAsync(string userIdOrUpn, DateTimeOffset start, DateTimeOffset end, int maxResults = 50, CancellationToken ct = default);
    Task UpdateEventAsync(string userIdOrUpn, string eventId, UpdateCalendarEvent patch, CancellationToken ct = default);
}