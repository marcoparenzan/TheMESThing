namespace TheItemsThingLib;

public interface IContactsService
{
    Task<User> CreateUserAsync(NewUser newUser, CancellationToken ct = default);
    Task DeleteUserAsync(string userIdOrUpn, CancellationToken ct = default);
    Task DisableUserAsync(string userIdOrUpn, CancellationToken ct = default);
    Task<User?> GetUserAsync(string userIdOrUpn, CancellationToken ct = default);
    Task<IReadOnlyList<User>> ListUsersAsync(string? filter = null, int maxResults = 100, CancellationToken ct = default);
    Task<IReadOnlyList<User>> SearchUsersAsync(string query, int maxResults = 25, CancellationToken ct = default);
    Task UpdateUserAsync(string userIdOrUpn, UpdateUser patch, CancellationToken ct = default);
}