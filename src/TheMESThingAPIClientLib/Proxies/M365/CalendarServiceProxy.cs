using TheItemsThingLib;

namespace TheMESThingAPIClientLib.Proxies.M365;

public sealed class CalendarServiceProxy(HttpClient http) : ProxyBase(http), ICalendarService
{
    public Task<IReadOnlyList<CalendarEvent>> ListEventsAsync(string userIdOrUpn, DateTimeOffset start, DateTimeOffset end, int maxResults = 50, CancellationToken ct = default) =>
        GetListAsync<CalendarEvent>(
            $"api/m365/calendar/{userIdOrUpn}/events?maxResults={maxResults}" +
            $"&start={Uri.EscapeDataString(start.ToString("o"))}" +
            $"&end={Uri.EscapeDataString(end.ToString("o"))}",
            ct);

    public Task<CalendarEvent?> GetEventAsync(string userIdOrUpn, string eventId, CancellationToken ct = default) =>
        GetNullableAsync<CalendarEvent>($"api/m365/calendar/{userIdOrUpn}/events/{eventId}", ct);

    public Task<CalendarEvent> CreateEventAsync(string userIdOrUpn, NewM365CalendarEvent newEvent, CancellationToken ct = default) =>
        PostAsync<NewM365CalendarEvent, CalendarEvent>($"api/m365/calendar/{userIdOrUpn}/events", newEvent, ct);

    public Task UpdateEventAsync(string userIdOrUpn, string eventId, UpdateCalendarEvent patch, CancellationToken ct = default) =>
        PutVoidAsync($"api/m365/calendar/{userIdOrUpn}/events/{eventId}", patch, ct);

    public Task DeleteEventAsync(string userIdOrUpn, string eventId, CancellationToken ct = default) =>
        DeleteVoidAsync($"api/m365/calendar/{userIdOrUpn}/events/{eventId}", ct);
}
