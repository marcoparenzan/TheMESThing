using Azure.Identity;
using Microsoft.Graph;
using Microsoft.Graph.Models;
using TheItemsThingLib;

namespace The365ThingLib;

public sealed class M365CalendarService: ICalendarService
{
    private readonly GraphServiceClient _graph;

    private static readonly string[] EventFields =
    [
        "id", "subject", "body", "start", "end", "location",
        "isAllDay", "isOnlineMeeting", "onlineMeetingUrl",
        "organizer", "attendees"
    ];

    public M365CalendarService(M365Config config)
    {
        var credential = new ClientSecretCredential(
            config.TenantId,
            config.ClientId,
            config.ClientSecret);

        _graph = new GraphServiceClient(credential,
            ["https://graph.microsoft.com/.default"]);
    }

    public async Task<CalendarEvent?> GetEventAsync(
        string userIdOrUpn, string eventId, CancellationToken ct = default)
    {
        try
        {
            var ev = await _graph.Users[userIdOrUpn].Events[eventId].GetAsync(req =>
            {
                req.QueryParameters.Select = EventFields;
                req.Headers.Add("Prefer", "outlook.timezone=\"UTC\"");
            }, ct);

            return ev is null ? null : MapEvent(ev);
        }
        catch
        {
            return null;
        }
    }

    public async Task<IReadOnlyList<CalendarEvent>> ListEventsAsync(
        string userIdOrUpn,
        DateTimeOffset start,
        DateTimeOffset end,
        int maxResults = 50,
        CancellationToken ct = default)
    {
        var response = await _graph.Users[userIdOrUpn].CalendarView.GetAsync(req =>
        {
            req.QueryParameters.StartDateTime = start.ToUniversalTime().ToString("o");
            req.QueryParameters.EndDateTime = end.ToUniversalTime().ToString("o");
            req.QueryParameters.Select = EventFields;
            req.QueryParameters.Top = maxResults;
            req.Headers.Add("Prefer", "outlook.timezone=\"UTC\"");
        }, ct);

        return response?.Value?.Select(MapEvent).ToList() ?? [];
    }

    public async Task<CalendarEvent> CreateEventAsync(
        string userIdOrUpn, NewM365CalendarEvent newEvent, CancellationToken ct = default)
    {
        var ev = new Event
        {
            Subject = newEvent.Subject,
            Start = ToDateTimeTimeZone(newEvent.Start),
            End = ToDateTimeTimeZone(newEvent.End),
            IsAllDay = newEvent.IsAllDay,
            IsOnlineMeeting = newEvent.IsOnlineMeeting,
        };

        if (newEvent.BodyContent is not null)
            ev.Body = new ItemBody { Content = newEvent.BodyContent, ContentType = BodyType.Text };

        if (newEvent.Location is not null)
            ev.Location = new Location { DisplayName = newEvent.Location };

        if (newEvent.AttendeeEmails.Count > 0)
            ev.Attendees = newEvent.AttendeeEmails
                .Select(email => new Attendee
                {
                    EmailAddress = new EmailAddress { Address = email },
                    Type = AttendeeType.Required
                })
                .ToList();

        var created = await _graph.Users[userIdOrUpn].Calendar.Events.PostAsync(ev, cancellationToken: ct)
            ?? throw new InvalidOperationException("Graph API returned null after event creation.");

        return MapEvent(created);
    }

    public async Task UpdateEventAsync(
        string userIdOrUpn, string eventId, UpdateCalendarEvent patch, CancellationToken ct = default)
    {
        var ev = new Event();

        if (patch.Subject is not null) ev.Subject = patch.Subject;
        if (patch.Start is not null) ev.Start = ToDateTimeTimeZone(patch.Start.Value);
        if (patch.End is not null) ev.End = ToDateTimeTimeZone(patch.End.Value);
        if (patch.IsAllDay is not null) ev.IsAllDay = patch.IsAllDay;
        if (patch.IsOnlineMeeting is not null) ev.IsOnlineMeeting = patch.IsOnlineMeeting;

        if (patch.BodyContent is not null)
            ev.Body = new ItemBody { Content = patch.BodyContent, ContentType = BodyType.Text };

        if (patch.Location is not null)
            ev.Location = new Location { DisplayName = patch.Location };

        if (patch.AttendeeEmails is not null)
            ev.Attendees = patch.AttendeeEmails
                .Select(email => new Attendee
                {
                    EmailAddress = new EmailAddress { Address = email },
                    Type = AttendeeType.Required
                })
                .ToList();

        await _graph.Users[userIdOrUpn].Events[eventId].PatchAsync(ev, cancellationToken: ct);
    }

    public async Task DeleteEventAsync(
        string userIdOrUpn, string eventId, CancellationToken ct = default)
    {
        await _graph.Users[userIdOrUpn].Events[eventId].DeleteAsync(cancellationToken: ct);
    }

    private static CalendarEvent MapEvent(Event ev) => new()
    {
        Id = ev.Id ?? string.Empty,
        Subject = ev.Subject,
        BodyContent = ev.Body?.Content,
        Start = ParseDateTime(ev.Start),
        End = ParseDateTime(ev.End),
        Location = ev.Location?.DisplayName,
        IsAllDay = ev.IsAllDay ?? false,
        IsOnlineMeeting = ev.IsOnlineMeeting ?? false,
        OnlineMeetingUrl = ev.OnlineMeetingUrl,
        OrganizerEmail = ev.Organizer?.EmailAddress?.Address,
        AttendeeEmails = ev.Attendees?
            .Select(a => a.EmailAddress?.Address ?? string.Empty)
            .Where(a => a.Length > 0)
            .ToList() ?? []
    };

    private static DateTimeTimeZone ToDateTimeTimeZone(DateTimeOffset dto) => new()
    {
        DateTime = dto.ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ss"),
        TimeZone = "UTC"
    };

    private static DateTimeOffset? ParseDateTime(DateTimeTimeZone? dtz)
    {
        if (dtz?.DateTime is null) return null;
        return DateTime.TryParse(dtz.DateTime, out var dt)
            ? new DateTimeOffset(dt, TimeSpan.Zero)
            : null;
    }
}
