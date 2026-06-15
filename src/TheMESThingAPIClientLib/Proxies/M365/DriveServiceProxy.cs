using TheItemsThingLib;

namespace TheMESThingAPIClientLib.Proxies.M365;

public sealed class DriveServiceProxy(HttpClient http) : ProxyBase(http), IDriveService
{
    public Task<IReadOnlyList<DriveItem>> ListRootItemsAsync(string userIdOrUpn, CancellationToken ct = default) =>
        GetListAsync<DriveItem>($"api/m365/drive/{userIdOrUpn}/items", ct);

    public Task<DriveItem?> GetItemAsync(string userIdOrUpn, string itemId, CancellationToken ct = default) =>
        GetNullableAsync<DriveItem>($"api/m365/drive/{userIdOrUpn}/items/{itemId}", ct);

    public Task<IReadOnlyList<DriveItem>> ListItemsByIdAsync(string userIdOrUpn, string itemId, CancellationToken ct = default) =>
        GetListAsync<DriveItem>($"api/m365/drive/{userIdOrUpn}/items/{itemId}/children", ct);

    public Task<byte[]?> DownloadFileAsync(string userIdOrUpn, string itemId, CancellationToken ct = default) =>
        GetBytesAsync($"api/m365/drive/{userIdOrUpn}/items/{itemId}/download", ct);

    public async Task<DriveItem?> CreateFolderAsync(string userIdOrUpn, string parentItemId, string folderName, CancellationToken ct = default) =>
        await PostAsync<TheMESThingAPIClientLib.Models.M365.CreateFolderRequest, DriveItem>(
            $"api/m365/drive/{userIdOrUpn}/items/{parentItemId}/folder",
            new TheMESThingAPIClientLib.Models.M365.CreateFolderRequest(folderName),
            ct);

    public async Task<DriveItem?> UploadFileAsync(string userIdOrUpn, string parentItemId, DriveUploadFile file, CancellationToken ct = default) =>
        await PostAsync<DriveUploadFile, DriveItem>(
            $"api/m365/drive/{userIdOrUpn}/items/{parentItemId}/upload",
            file,
            ct);

    public Task<bool> DeleteItemAsync(string userIdOrUpn, string itemId, CancellationToken ct = default) =>
        DeleteBoolAsync($"api/m365/drive/{userIdOrUpn}/items/{itemId}", ct);
}
