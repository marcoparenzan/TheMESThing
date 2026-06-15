using TheItemsThingLib;

namespace TheMESThingAPIClientLib.Proxies.M365;

public sealed class EmailServiceProxy(HttpClient http) : ProxyBase(http), IEmailService
{
    public Task<IReadOnlyList<Email>> ListInboxEmailsAsync(string userIdOrUpn, int maxResults = 50, CancellationToken ct = default) =>
        GetListAsync<Email>($"api/m365/email/{userIdOrUpn}/inbox?maxResults={maxResults}", ct);

    public Task<IReadOnlyList<Email>> ListEmailsAsync(string userIdOrUpn, int maxResults = 50, CancellationToken ct = default) =>
        GetListAsync<Email>($"api/m365/email/{userIdOrUpn}/messages?maxResults={maxResults}", ct);

    public Task<IReadOnlyList<MailFolder>> ListMailFoldersAsync(string userIdOrUpn, CancellationToken ct = default) =>
        GetListAsync<MailFolder>($"api/m365/email/{userIdOrUpn}/folders", ct);

    public Task<IReadOnlyList<Email>> ListEmailsByFolderIdAsync(string userIdOrUpn, string folderId, int maxResults = 50, CancellationToken ct = default) =>
        GetListAsync<Email>($"api/m365/email/{userIdOrUpn}/folders/{folderId}/messages?maxResults={maxResults}", ct);

    public Task<Email?> GetEmailAsync(string userIdOrUpn, string messageId, CancellationToken ct = default) =>
        GetNullableAsync<Email>($"api/m365/email/{userIdOrUpn}/messages/{messageId}", ct);

    public Task<Email> SendEmailAsync(string userIdOrUpn, NewEmail newEmail, CancellationToken ct = default) =>
        PostAsync<NewEmail, Email>($"api/m365/email/{userIdOrUpn}/messages", newEmail, ct);

    public Task SendEmailAndSaveAsync(string userIdOrUpn, NewEmail newEmail, CancellationToken ct = default) =>
        PostVoidAsync($"api/m365/email/{userIdOrUpn}/messages/send-and-save", newEmail, ct);

    public Task MarkEmailAsReadAsync(string userIdOrUpn, string messageId, CancellationToken ct = default) =>
        PutVoidAsync($"api/m365/email/{userIdOrUpn}/messages/{messageId}/read", ct);

    public Task DeleteEmailAsync(string userIdOrUpn, string messageId, CancellationToken ct = default) =>
        DeleteVoidAsync($"api/m365/email/{userIdOrUpn}/messages/{messageId}", ct);
}
