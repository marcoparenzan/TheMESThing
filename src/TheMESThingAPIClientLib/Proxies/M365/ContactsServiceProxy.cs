using TheItemsThingLib;

namespace TheMESThingAPIClientLib.Proxies.M365;

public sealed class ContactsServiceProxy(HttpClient http) : ProxyBase(http), IContactsService
{
    public Task<IReadOnlyList<User>> ListUsersAsync(string? filter = null, int maxResults = 100, CancellationToken ct = default)
    {
        var url = $"api/m365/users?maxResults={maxResults}";
        if (filter != null) url += $"&filter={Uri.EscapeDataString(filter)}";
        return GetListAsync<User>(url, ct);
    }

    public Task<IReadOnlyList<User>> SearchUsersAsync(string query, int maxResults = 25, CancellationToken ct = default) =>
        GetListAsync<User>($"api/m365/users/search?query={Uri.EscapeDataString(query)}&maxResults={maxResults}", ct);

    public Task<User?> GetUserAsync(string userIdOrUpn, CancellationToken ct = default) =>
        GetNullableAsync<User>($"api/m365/users/{Uri.EscapeDataString(userIdOrUpn)}", ct);

    public Task<User> CreateUserAsync(NewUser newUser, CancellationToken ct = default) =>
        PostAsync<NewUser, User>("api/m365/users", newUser, ct);

    public Task UpdateUserAsync(string userIdOrUpn, UpdateUser patch, CancellationToken ct = default) =>
        PutVoidAsync($"api/m365/users/{Uri.EscapeDataString(userIdOrUpn)}", patch, ct);

    public Task DisableUserAsync(string userIdOrUpn, CancellationToken ct = default) =>
        PutVoidAsync($"api/m365/users/{Uri.EscapeDataString(userIdOrUpn)}/disable", ct);

    public Task DeleteUserAsync(string userIdOrUpn, CancellationToken ct = default) =>
        DeleteVoidAsync($"api/m365/users/{Uri.EscapeDataString(userIdOrUpn)}", ct);
}
