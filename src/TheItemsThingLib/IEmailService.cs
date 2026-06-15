namespace TheItemsThingLib;

public interface IEmailService
{
    Task DeleteEmailAsync(string userIdOrUpn, string messageId, CancellationToken ct = default);
    Task<Email?> GetEmailAsync(string userIdOrUpn, string messageId, CancellationToken ct = default);
    Task<IReadOnlyList<Email>> ListEmailsAsync(string userIdOrUpn, int maxResults = 50, CancellationToken ct = default);
    Task<IReadOnlyList<Email>> ListEmailsByFolderIdAsync(string userIdOrUpn, string folderId, int maxResults = 50, CancellationToken ct = default);
    Task<IReadOnlyList<Email>> ListInboxEmailsAsync(string userIdOrUpn, int maxResults = 50, CancellationToken ct = default);
    Task<IReadOnlyList<MailFolder>> ListMailFoldersAsync(string userIdOrUpn, CancellationToken ct = default);
    Task MarkEmailAsReadAsync(string userIdOrUpn, string messageId, CancellationToken ct = default);
    Task SendEmailAndSaveAsync(string userIdOrUpn, NewEmail newEmail, CancellationToken ct = default);
    Task<Email> SendEmailAsync(string userIdOrUpn, NewEmail newEmail, CancellationToken ct = default);
}