using Microsoft.AspNetCore.Http.HttpResults;
using TheItemsThingLib;

namespace TheMESThingAPI.Endpoints;

public static class M365Endpoints
{
    public static IEndpointRouteBuilder MapM365Endpoints(this IEndpointRouteBuilder app)
    {
        MapEmail(app);
        MapCalendar(app);
        MapUsers(app);
        MapDrive(app);
        return app;
    }

    // ── Email ──────────────────────────────────────────────────────────────────
    static void MapEmail(IEndpointRouteBuilder app)
    {
        var g = app.MapGroup("/api/m365/email").WithTags("M365 – Email");

        g.MapGet("/{userId}/inbox", async (string userId, IEmailService svc, int maxResults = 50, CancellationToken ct = default) =>
        {
            var emails = await svc.ListInboxEmailsAsync(userId, maxResults, ct);
            return TypedResults.Ok(emails);
        })
        .WithName("ListInboxEmails").WithSummary("List inbox emails for a user");

        g.MapGet("/{userId}/messages", async (string userId, IEmailService svc, int maxResults = 50, CancellationToken ct = default) =>
        {
            var emails = await svc.ListEmailsAsync(userId, maxResults, ct);
            return TypedResults.Ok(emails);
        })
        .WithName("ListEmails").WithSummary("List all emails for a user");

        g.MapGet("/{userId}/folders", async (string userId, IEmailService svc, CancellationToken ct = default) =>
        {
            var folders = await svc.ListMailFoldersAsync(userId, ct);
            return TypedResults.Ok(folders);
        })
        .WithName("ListMailFolders").WithSummary("List mail folders for a user");

        g.MapGet("/{userId}/folders/{folderId}/messages", async (string userId, string folderId, IEmailService svc, int maxResults = 50, CancellationToken ct = default) =>
        {
            var emails = await svc.ListEmailsByFolderIdAsync(userId, folderId, maxResults, ct);
            return TypedResults.Ok(emails);
        })
        .WithName("ListEmailsByFolder").WithSummary("List emails in a specific folder");

        g.MapGet("/{userId}/messages/{messageId}", async Task<Results<Ok<Email>, NotFound>> (string userId, string messageId, IEmailService svc, CancellationToken ct = default) =>
        {
            var email = await svc.GetEmailAsync(userId, messageId, ct);
            return email is null ? TypedResults.NotFound() : TypedResults.Ok(email);
        })
        .WithName("GetEmail").WithSummary("Get an email by ID");

        g.MapPost("/{userId}/messages", async Task<Ok<Email>> (string userId, NewEmail body, IEmailService svc, CancellationToken ct = default) =>
        {
            var sent = await svc.SendEmailAsync(userId, body, ct);
            return TypedResults.Ok(sent);
        })
        .WithName("SendEmail").WithSummary("Send an email");

        g.MapPost("/{userId}/messages/send-and-save", async (string userId, NewEmail body, IEmailService svc, CancellationToken ct = default) =>
        {
            await svc.SendEmailAndSaveAsync(userId, body, ct);
            return TypedResults.NoContent();
        })
        .WithName("SendEmailAndSave").WithSummary("Send an email and save it to Sent Items");

        g.MapPut("/{userId}/messages/{messageId}/read", async (string userId, string messageId, IEmailService svc, CancellationToken ct = default) =>
        {
            await svc.MarkEmailAsReadAsync(userId, messageId, ct);
            return TypedResults.NoContent();
        })
        .WithName("MarkEmailAsRead").WithSummary("Mark an email as read");

        g.MapDelete("/{userId}/messages/{messageId}", async (string userId, string messageId, IEmailService svc, CancellationToken ct = default) =>
        {
            await svc.DeleteEmailAsync(userId, messageId, ct);
            return TypedResults.NoContent();
        })
        .WithName("DeleteEmail").WithSummary("Delete an email");
    }

    // ── Calendar ───────────────────────────────────────────────────────────────
    static void MapCalendar(IEndpointRouteBuilder app)
    {
        var g = app.MapGroup("/api/m365/calendar").WithTags("M365 – Calendar");

        g.MapGet("/{userId}/events", async (string userId, ICalendarService svc, DateTimeOffset? start, DateTimeOffset? end, int maxResults = 50, CancellationToken ct = default) =>
        {
            var s = start ?? DateTimeOffset.UtcNow;
            var e = end ?? DateTimeOffset.UtcNow.AddMonths(1);
            var events = await svc.ListEventsAsync(userId, s, e, maxResults, ct);
            return TypedResults.Ok(events);
        })
        .WithName("ListCalendarEvents").WithSummary("List calendar events for a user");

        g.MapGet("/{userId}/events/{eventId}", async Task<Results<Ok<CalendarEvent>, NotFound>> (string userId, string eventId, ICalendarService svc, CancellationToken ct = default) =>
        {
            var ev = await svc.GetEventAsync(userId, eventId, ct);
            return ev is null ? TypedResults.NotFound() : TypedResults.Ok(ev);
        })
        .WithName("GetCalendarEvent").WithSummary("Get a calendar event by ID");

        g.MapPost("/{userId}/events", async Task<Created<CalendarEvent>> (string userId, NewM365CalendarEvent body, ICalendarService svc, CancellationToken ct = default) =>
        {
            var created = await svc.CreateEventAsync(userId, body, ct);
            return TypedResults.Created($"/api/m365/calendar/{userId}/events/{created.Id}", created);
        })
        .WithName("CreateCalendarEvent").WithSummary("Create a calendar event");

        g.MapPut("/{userId}/events/{eventId}", async (string userId, string eventId, UpdateCalendarEvent body, ICalendarService svc, CancellationToken ct = default) =>
        {
            await svc.UpdateEventAsync(userId, eventId, body, ct);
            return TypedResults.NoContent();
        })
        .WithName("UpdateCalendarEvent").WithSummary("Update a calendar event");

        g.MapDelete("/{userId}/events/{eventId}", async (string userId, string eventId, ICalendarService svc, CancellationToken ct = default) =>
        {
            await svc.DeleteEventAsync(userId, eventId, ct);
            return TypedResults.NoContent();
        })
        .WithName("DeleteCalendarEvent").WithSummary("Delete a calendar event");
    }

    // ── Users ──────────────────────────────────────────────────────────────────
    static void MapUsers(IEndpointRouteBuilder app)
    {
        var g = app.MapGroup("/api/m365/users").WithTags("M365 – Users");

        g.MapGet("/", async (IContactsService svc, string? filter, int maxResults = 100, CancellationToken ct = default) =>
        {
            var users = await svc.ListUsersAsync(filter, maxResults, ct);
            return TypedResults.Ok(users);
        })
        .WithName("ListM365Users").WithSummary("List M365 users");

        g.MapGet("/search", async (IContactsService svc, string query, int maxResults = 25, CancellationToken ct = default) =>
        {
            var users = await svc.SearchUsersAsync(query, maxResults, ct);
            return TypedResults.Ok(users);
        })
        .WithName("SearchM365Users").WithSummary("Search M365 users by display name or email");

        g.MapGet("/{userIdOrUpn}", async Task<Results<Ok<TheItemsThingLib.User>, NotFound>> (string userIdOrUpn, IContactsService svc, CancellationToken ct = default) =>
        {
            var user = await svc.GetUserAsync(userIdOrUpn, ct);
            return user is null ? TypedResults.NotFound() : TypedResults.Ok(user);
        })
        .WithName("GetM365User").WithSummary("Get an M365 user by ID or UPN");

        g.MapPost("/", async Task<Created<TheItemsThingLib.User>> (NewUser body, IContactsService svc, CancellationToken ct = default) =>
        {
            var created = await svc.CreateUserAsync(body, ct);
            return TypedResults.Created($"/api/m365/users/{created.Id}", created);
        })
        .WithName("CreateM365User").WithSummary("Create an M365 user");

        g.MapPut("/{userIdOrUpn}", async (string userIdOrUpn, UpdateUser body, IContactsService svc, CancellationToken ct = default) =>
        {
            await svc.UpdateUserAsync(userIdOrUpn, body, ct);
            return TypedResults.NoContent();
        })
        .WithName("UpdateM365User").WithSummary("Update an M365 user");

        g.MapPut("/{userIdOrUpn}/disable", async (string userIdOrUpn, IContactsService svc, CancellationToken ct = default) =>
        {
            await svc.DisableUserAsync(userIdOrUpn, ct);
            return TypedResults.NoContent();
        })
        .WithName("DisableM365User").WithSummary("Disable an M365 user account");

        g.MapDelete("/{userIdOrUpn}", async (string userIdOrUpn, IContactsService svc, CancellationToken ct = default) =>
        {
            await svc.DeleteUserAsync(userIdOrUpn, ct);
            return TypedResults.NoContent();
        })
        .WithName("DeleteM365User").WithSummary("Delete an M365 user");
    }

    // ── Drive ──────────────────────────────────────────────────────────────────
    static void MapDrive(IEndpointRouteBuilder app)
    {
        var g = app.MapGroup("/api/m365/drive").WithTags("M365 – Drive");

        g.MapGet("/{userId}/items", async (string userId, IDriveService svc, CancellationToken ct = default) =>
        {
            var items = await svc.ListRootItemsAsync(userId, ct);
            return TypedResults.Ok(items);
        })
        .WithName("ListDriveRootItems").WithSummary("List root OneDrive items for a user");

        g.MapGet("/{userId}/items/{itemId}", async Task<Results<Ok<DriveItem>, NotFound>> (string userId, string itemId, IDriveService svc, CancellationToken ct = default) =>
        {
            var item = await svc.GetItemAsync(userId, itemId, ct);
            return item is null ? TypedResults.NotFound() : TypedResults.Ok(item);
        })
        .WithName("GetDriveItem").WithSummary("Get a OneDrive item by ID");

        g.MapGet("/{userId}/items/{itemId}/children", async (string userId, string itemId, IDriveService svc, CancellationToken ct = default) =>
        {
            var items = await svc.ListItemsByIdAsync(userId, itemId, ct);
            return TypedResults.Ok(items);
        })
        .WithName("ListDriveItemChildren").WithSummary("List children of a OneDrive folder");

        g.MapGet("/{userId}/items/{itemId}/download", async Task<Results<FileContentHttpResult, NotFound>> (string userId, string itemId, IDriveService svc, CancellationToken ct = default) =>
        {
            var bytes = await svc.DownloadFileAsync(userId, itemId, ct);
            return bytes is null ? TypedResults.NotFound() : TypedResults.File(bytes, "application/octet-stream");
        })
        .WithName("DownloadDriveFile").WithSummary("Download a file from OneDrive");

        g.MapPost("/{userId}/items/{parentItemId}/folder", async Task<Results<Ok<DriveItem>, NotFound>> (string userId, string parentItemId, CreateFolderRequest body, IDriveService svc, CancellationToken ct = default) =>
        {
            var item = await svc.CreateFolderAsync(userId, parentItemId, body.FolderName, ct);
            return item is null ? TypedResults.NotFound() : TypedResults.Ok(item);
        })
        .WithName("CreateDriveFolder").WithSummary("Create a folder in OneDrive");

        g.MapPost("/{userId}/items/{parentItemId}/upload", async Task<Results<Ok<DriveItem>, NotFound>> (string userId, string parentItemId, DriveUploadFile body, IDriveService svc, CancellationToken ct = default) =>
        {
            var item = await svc.UploadFileAsync(userId, parentItemId, body, ct);
            return item is null ? TypedResults.NotFound() : TypedResults.Ok(item);
        })
        .WithName("UploadDriveFile").WithSummary("Upload a file to OneDrive");

        g.MapDelete("/{userId}/items/{itemId}", async Task<Results<NoContent, NotFound>> (string userId, string itemId, IDriveService svc, CancellationToken ct = default) =>
        {
            var deleted = await svc.DeleteItemAsync(userId, itemId, ct);
            return deleted ? TypedResults.NoContent() : TypedResults.NotFound();
        })
        .WithName("DeleteDriveItem").WithSummary("Delete a OneDrive item");
    }
}

public sealed record CreateFolderRequest(string FolderName);
