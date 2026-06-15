using System.Collections.Concurrent;
using Azure.Identity;
using Microsoft.Graph;
using Microsoft.Graph.Models;
using TheItemsThingLib;

namespace The365ThingLib;

public sealed class M365DriveService: IDriveService
{
    private readonly GraphServiceClient _graph;
    private readonly ConcurrentDictionary<string, string> _driveIdCache = new();

    private static readonly string[] DriveItemFields =
    [
        "id", "name", "webUrl", "size", "folder", "file",
        "createdDateTime", "lastModifiedDateTime", "parentReference"
    ];

    public M365DriveService(M365Config config)
    {
        var credential = new ClientSecretCredential(
            config.TenantId,
            config.ClientId,
            config.ClientSecret);

        _graph = new GraphServiceClient(credential,
            ["https://graph.microsoft.com/.default"]);
    }

    // Resolves and caches the user's default drive ID.
    // Graph v5.105 removed .Drive.Items — all item operations must go through
    // _graph.Drives[driveId].Items[...], which requires the drive ID.
    private async Task<string> GetDriveIdAsync(string userIdOrUpn, CancellationToken ct)
    {
        if (_driveIdCache.TryGetValue(userIdOrUpn, out var cached))
            return cached;

        // .Drive (singular) maps to GET /users/{id}/drive — avoids the MySite lookup
        // that .Drives (plural) triggers, which fails if OneDrive is not yet provisioned.
        var drive = await _graph.Users[userIdOrUpn].Drive.GetAsync(cancellationToken: ct);
        var id = drive?.Id ?? throw new InvalidOperationException($"Drive not found for user '{userIdOrUpn}'.");
        _driveIdCache[userIdOrUpn] = id;
        return id;
        }

    public async Task<IReadOnlyList<TheItemsThingLib.DriveItem>> ListRootItemsAsync(
        string userIdOrUpn, CancellationToken ct = default)
        => await ListItemsByIdAsync(userIdOrUpn, "root", ct);

    public async Task<IReadOnlyList<TheItemsThingLib.DriveItem>> ListItemsByIdAsync(
        string userIdOrUpn, string itemId, CancellationToken ct = default)
    {
        try
        {
            var driveId = await GetDriveIdAsync(userIdOrUpn, ct);
            var response = await _graph.Drives[driveId].Items[itemId].Children.GetAsync(req =>
            {
                req.QueryParameters.Select = DriveItemFields;
                req.QueryParameters.Orderby = ["name"];
            }, ct);

            return response?.Value?.Select(MapDriveItem).ToList().AsReadOnly()
                ?? (IReadOnlyList<TheItemsThingLib.DriveItem>)[];
        }
        catch(Exception ex)
        {
            return [];
        }
    }

    public async Task<TheItemsThingLib.DriveItem?> GetItemAsync(
        string userIdOrUpn, string itemId, CancellationToken ct = default)
    {
        try
        {
            var driveId = await GetDriveIdAsync(userIdOrUpn, ct);
            var item = await _graph.Drives[driveId].Items[itemId].GetAsync(req =>
            {
                req.QueryParameters.Select = DriveItemFields;
            }, ct);

            return item is null ? null : MapDriveItem(item);
        }
        catch
        {
            return null;
        }
    }

    public async Task<byte[]?> DownloadFileAsync(
        string userIdOrUpn, string itemId, CancellationToken ct = default)
    {
        try
        {
            var driveId = await GetDriveIdAsync(userIdOrUpn, ct);
            var stream = await _graph.Drives[driveId].Items[itemId].Content.GetAsync(cancellationToken: ct);
            if (stream is null) return null;
            using var ms = new MemoryStream();
            await stream.CopyToAsync(ms, ct);
            return ms.ToArray();
        }
        catch
        {
            return null;
        }
    }

    // Upload uses the path-based item address: {parentItemId}:/{fileName}:
    // which maps to PUT /drives/{driveId}/items/{parentItemId}:/{fileName}:/content
    public async Task<TheItemsThingLib.DriveItem?> UploadFileAsync(
        string userIdOrUpn, string parentItemId, DriveUploadFile file,
        CancellationToken ct = default)
    {
        try
        {
            var driveId   = await GetDriveIdAsync(userIdOrUpn, ct);
            var itemPath  = $"{parentItemId}:/{file.Name}:";
            using var stream = new MemoryStream(file.Content);
            var uploaded = await _graph.Drives[driveId].Items[itemPath].Content.PutAsync(stream, cancellationToken: ct);
            return uploaded is null ? null : MapDriveItem(uploaded);
        }
        catch
        {
            return null;
        }
    }

    public async Task<TheItemsThingLib.DriveItem?> CreateFolderAsync(
        string userIdOrUpn, string parentItemId, string folderName,
        CancellationToken ct = default)
    {
        try
        {
            var driveId = await GetDriveIdAsync(userIdOrUpn, ct);
            var created = await _graph.Drives[driveId].Items[parentItemId].Children.PostAsync(
                new Microsoft.Graph.Models.DriveItem { Name = folderName, Folder = new Folder() }, cancellationToken: ct);
            return created is null ? null : MapDriveItem(created);
        }
        catch
        {
            return null;
        }
    }

    public async Task<bool> DeleteItemAsync(
        string userIdOrUpn, string itemId, CancellationToken ct = default)
    {
        try
        {
            var driveId = await GetDriveIdAsync(userIdOrUpn, ct);
            await _graph.Drives[driveId].Items[itemId].DeleteAsync(cancellationToken: ct);
            return true;
        }
        catch
        {
            return false;
        }
    }

    private static TheItemsThingLib.DriveItem MapDriveItem(Microsoft.Graph.Models.DriveItem item) => new()
    {
        Id                = item.Id ?? string.Empty,
        Name              = item.Name ?? "Unknown",
        DisplayPath       = item.ParentReference?.Path,
        IsFolder          = item.Folder != null,
        Size              = item.Size,
        CreatedDateTime   = item.CreatedDateTime,
        ModifiedDateTime  = item.LastModifiedDateTime,
        WebUrl            = item.WebUrl,
        ParentId          = item.ParentReference?.Id,
        MimeType          = item.File?.MimeType
    };
}
