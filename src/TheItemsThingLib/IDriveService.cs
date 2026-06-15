namespace TheItemsThingLib;

public interface IDriveService
{
    Task<DriveItem?> CreateFolderAsync(string userIdOrUpn, string parentItemId, string folderName, CancellationToken ct = default);
    Task<bool> DeleteItemAsync(string userIdOrUpn, string itemId, CancellationToken ct = default);
    Task<byte[]?> DownloadFileAsync(string userIdOrUpn, string itemId, CancellationToken ct = default);
    Task<DriveItem?> GetItemAsync(string userIdOrUpn, string itemId, CancellationToken ct = default);
    Task<IReadOnlyList<DriveItem>> ListItemsByIdAsync(string userIdOrUpn, string itemId, CancellationToken ct = default);
    Task<IReadOnlyList<DriveItem>> ListRootItemsAsync(string userIdOrUpn, CancellationToken ct = default);
    Task<DriveItem?> UploadFileAsync(string userIdOrUpn, string parentItemId, DriveUploadFile file, CancellationToken ct = default);
}