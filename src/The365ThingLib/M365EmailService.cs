using Azure.Identity;
using Microsoft.Graph;
using Microsoft.Graph.Models;
using Microsoft.Graph.Users.Item.SendMail;
using TheItemsThingLib;

namespace The365ThingLib;

public sealed class M365EmailService: IEmailService
{
    private readonly GraphServiceClient _graph;

    private static readonly string[] EmailFields =
    [
        "id", "subject", "body", "from", "toRecipients", "ccRecipients",
        "receivedDateTime", "isRead", "hasAttachments"
    ];

    public M365EmailService(M365Config config)
    {
        var credential = new ClientSecretCredential(
            config.TenantId,
            config.ClientId,
            config.ClientSecret);

        _graph = new GraphServiceClient(credential,
            ["https://graph.microsoft.com/.default"]);
    }

    public async Task<Email?> GetEmailAsync(
        string userIdOrUpn, string messageId, CancellationToken ct = default)
    {
        try
        {
            var message = await _graph.Users[userIdOrUpn].Messages[messageId].GetAsync(req =>
            {
                req.QueryParameters.Select = EmailFields;
            }, ct);

            return message is null ? null : MapEmail(message);
        }
        catch
        {
            return null;
        }
    }

    public async Task<IReadOnlyList<Email>> ListEmailsAsync(
        string userIdOrUpn,
        int maxResults = 50,
        CancellationToken ct = default)
    {
        try
        {
            var response = await _graph.Users[userIdOrUpn].Messages.GetAsync(req =>
            {
                req.QueryParameters.Select = EmailFields;
                req.QueryParameters.Top = maxResults;
                req.QueryParameters.Orderby = ["receivedDateTime desc"];
            }, ct);

            return response?.Value?.Select(MapEmail).ToList() ?? [];
        }
        catch
        {
            return [];
        }
    }

    public async Task<IReadOnlyList<Email>> ListInboxEmailsAsync(
        string userIdOrUpn,
        int maxResults = 50,
        CancellationToken ct = default)
    {
        try
        {
            var response = await _graph.Users[userIdOrUpn].MailFolders["inbox"].Messages.GetAsync(req =>
            {
                req.QueryParameters.Select = EmailFields;
                req.QueryParameters.Top = maxResults;
                req.QueryParameters.Orderby = ["receivedDateTime desc"];
            }, ct);

            return response?.Value?.Select(MapEmail).ToList() ?? [];
        }
        catch
        {
            return [];
        }
    }

    public async Task<Email> SendEmailAsync(
        string userIdOrUpn, NewEmail newEmail, CancellationToken ct = default)
    {
        var message = new Message
        {
            Subject = newEmail.Subject,
            Body = new ItemBody
            {
                Content = newEmail.BodyContent,
                ContentType = newEmail.IsHtml ? BodyType.Html : BodyType.Text
            },
            ToRecipients = newEmail.ToEmails
                .Select(email => new Recipient
                {
                    EmailAddress = new EmailAddress { Address = email }
                })
                .ToList(),
            CcRecipients = newEmail.CcEmails
                .Select(email => new Recipient
                {
                    EmailAddress = new EmailAddress { Address = email }
                })
                .ToList(),
            BccRecipients = newEmail.BccEmails
                .Select(email => new Recipient
                {
                    EmailAddress = new EmailAddress { Address = email }
                })
                .ToList()
        };

        var savedMessage = await _graph.Users[userIdOrUpn].Messages.PostAsync(message, cancellationToken: ct)
            ?? throw new InvalidOperationException("Graph API returned null after message creation.");

        return MapEmail(savedMessage);
    }

    public async Task SendEmailAndSaveAsync(
        string userIdOrUpn, NewEmail newEmail, CancellationToken ct = default)
    {
        var message = new Message
        {
            Subject = newEmail.Subject,
            Body = new ItemBody
            {
                Content = newEmail.BodyContent,
                ContentType = newEmail.IsHtml ? BodyType.Html : BodyType.Text
            },
            ToRecipients = newEmail.ToEmails
                .Select(email => new Recipient
                {
                    EmailAddress = new EmailAddress { Address = email }
                })
                .ToList(),
            CcRecipients = newEmail.CcEmails
                .Select(email => new Recipient
                {
                    EmailAddress = new EmailAddress { Address = email }
                })
                .ToList(),
            BccRecipients = newEmail.BccEmails
                .Select(email => new Recipient
                {
                    EmailAddress = new EmailAddress { Address = email }
                })
                .ToList()
        };

        var requestBody = new SendMailPostRequestBody { Message = message };
        await _graph.Users[userIdOrUpn].SendMail.PostAsync(requestBody, cancellationToken: ct);
    }

    public async Task<IReadOnlyList<TheItemsThingLib.MailFolder>> ListMailFoldersAsync(
        string userIdOrUpn, CancellationToken ct = default)
    {
        try
        {
            var response = await _graph.Users[userIdOrUpn].MailFolders.GetAsync(req =>
            {
                req.QueryParameters.Select = ["id", "displayName", "unreadItemCount", "totalItemCount"];
                req.QueryParameters.Top = 50;
            }, ct);

            return response?.Value?
                .Select(f => new TheItemsThingLib.MailFolder
                {
                    Id              = f.Id ?? string.Empty,
                    DisplayName     = f.DisplayName ?? "Unknown",
                    UnreadItemCount = f.UnreadItemCount ?? 0,
                    TotalItemCount  = f.TotalItemCount ?? 0
                })
                .ToList() ?? [];
        }
        catch
        {
            return [];
        }
    }

    public async Task<IReadOnlyList<Email>> ListEmailsByFolderIdAsync(
        string userIdOrUpn, string folderId,
        int maxResults = 50,
        CancellationToken ct = default)
    {
        try
        {
            var response = await _graph.Users[userIdOrUpn].MailFolders[folderId].Messages.GetAsync(req =>
            {
                req.QueryParameters.Select = EmailFields;
                req.QueryParameters.Top = maxResults;
                req.QueryParameters.Orderby = ["receivedDateTime desc"];
            }, ct);

            return response?.Value?.Select(MapEmail).ToList() ?? [];
        }
        catch
        {
            return [];
        }
    }

    public async Task DeleteEmailAsync(
        string userIdOrUpn, string messageId, CancellationToken ct = default)
    {
        await _graph.Users[userIdOrUpn].Messages[messageId].DeleteAsync(cancellationToken: ct);
    }

    public async Task MarkEmailAsReadAsync(
        string userIdOrUpn, string messageId, CancellationToken ct = default)
    {
        var message = new Message { IsRead = true };
        await _graph.Users[userIdOrUpn].Messages[messageId].PatchAsync(message, cancellationToken: ct);
    }

    private static Email MapEmail(Message msg) => new()
    {
        Id = msg.Id ?? string.Empty,
        Subject = msg.Subject,
        BodyContent = msg.Body?.Content,
        FromEmail = msg.From?.EmailAddress?.Address,
        FromName = msg.From?.EmailAddress?.Name,
        ToEmails = msg.ToRecipients?
            .Select(r => r.EmailAddress?.Address ?? string.Empty)
            .Where(a => a.Length > 0)
            .ToList() ?? [],
        CcEmails = msg.CcRecipients?
            .Select(r => r.EmailAddress?.Address ?? string.Empty)
            .Where(a => a.Length > 0)
            .ToList() ?? [],
        ReceivedDateTime = msg.ReceivedDateTime,
        IsRead = msg.IsRead ?? false,
        HasAttachments = msg.HasAttachments ?? false
    };
}
