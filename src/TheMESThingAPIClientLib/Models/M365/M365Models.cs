namespace TheMESThingAPIClientLib.Models.M365;

public record Email(
    string Id,
    string? Subject,
    string? BodyContent,
    string? FromEmail,
    string? FromName,
    IReadOnlyList<string> ToEmails,
    IReadOnlyList<string> CcEmails,
    DateTimeOffset? ReceivedDateTime,
    bool IsRead,
    bool HasAttachments);

public record NewEmail(
    string Subject,
    string BodyContent,
    IReadOnlyList<string> ToEmails,
    IReadOnlyList<string> CcEmails,
    IReadOnlyList<string> BccEmails,
    bool IsHtml);

public record MailFolder(
    string Id,
    string DisplayName,
    int UnreadItemCount,
    int TotalItemCount);

public record CalendarEvent(
    string Id,
    string? Subject,
    string? BodyContent,
    DateTimeOffset? Start,
    DateTimeOffset? End,
    string? Location,
    bool IsAllDay,
    bool IsOnlineMeeting,
    string? OnlineMeetingUrl,
    string? OrganizerEmail,
    IReadOnlyList<string> AttendeeEmails);

public record NewM365CalendarEvent(
    string Subject,
    DateTimeOffset Start,
    DateTimeOffset End,
    string? BodyContent,
    string? Location,
    bool IsAllDay,
    bool IsOnlineMeeting,
    IReadOnlyList<string> AttendeeEmails);

public record UpdateCalendarEvent(
    string? Subject,
    DateTimeOffset? Start,
    DateTimeOffset? End,
    string? BodyContent,
    string? Location,
    bool? IsAllDay,
    bool? IsOnlineMeeting,
    IReadOnlyList<string>? AttendeeEmails);

public record M365User(
    string Id,
    string? DisplayName,
    string? GivenName,
    string? Surname,
    string? Mail,
    string? UserPrincipalName,
    string? JobTitle,
    string? Department,
    string? MobilePhone,
    string? OfficeLocation,
    bool AccountEnabled);

public record NewUser(
    string DisplayName,
    string UserPrincipalName,
    string MailNickname,
    string Password,
    string? GivenName,
    string? Surname,
    string? JobTitle,
    string? Department,
    string? MobilePhone,
    string? OfficeLocation,
    bool AccountEnabled,
    bool ForceChangePasswordNextSignIn);

public record UpdateUser(
    string? DisplayName,
    string? GivenName,
    string? Surname,
    string? JobTitle,
    string? Department,
    string? MobilePhone,
    string? OfficeLocation,
    bool? AccountEnabled);

public record DriveItem(
    string Id,
    string Name,
    string? DisplayPath,
    bool IsFolder,
    long? Size,
    DateTimeOffset? CreatedDateTime,
    DateTimeOffset? ModifiedDateTime,
    string? WebUrl,
    string? ParentId,
    string? MimeType);

public record DriveUploadFile(
    string Name,
    byte[] Content,
    string MimeType);

public record CreateFolderRequest(string FolderName);
