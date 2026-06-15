using Azure.Identity;
using Microsoft.Graph;
using Microsoft.Graph.Models;
using TheItemsThingLib;

namespace The365ThingLib;

public sealed class M365ContactsService: IContactsService
{
    private readonly GraphServiceClient _graph;

    private static readonly string[] UserFields =
    [
        "id", "displayName", "givenName", "surname", "mail",
        "userPrincipalName", "jobTitle", "department",
        "mobilePhone", "officeLocation", "accountEnabled"
    ];

    public M365ContactsService(M365Config config)
    {
        var credential = new ClientSecretCredential(
            config.TenantId,
            config.ClientId,
            config.ClientSecret);

        _graph = new GraphServiceClient(credential,
            ["https://graph.microsoft.com/.default"]);
    }

    public async Task<TheItemsThingLib.User?> GetUserAsync(string userIdOrUpn, CancellationToken ct = default)
    {
        try
        {
            var user = await _graph.Users[userIdOrUpn].GetAsync(req =>
            {
                req.QueryParameters.Select = UserFields;
            }, ct);

            return user is null ? null : MapUser(user);
        }
        catch (Exception ex)
        {
            return null;
        }
    }

    public async Task<IReadOnlyList<TheItemsThingLib.User>> ListUsersAsync(
        string? filter = null,
        int maxResults = 100,
        CancellationToken ct = default)
    {
        var response = await _graph.Users.GetAsync(req =>
        {
            req.QueryParameters.Select = UserFields;
            req.QueryParameters.Top = maxResults;
            if (filter is not null)
                req.QueryParameters.Filter = filter;
            req.QueryParameters.Orderby = ["displayName"];
        }, ct);

        return response?.Value?.Select(MapUser).ToList() ?? [];
    }

    public async Task<IReadOnlyList<TheItemsThingLib.User>> SearchUsersAsync(
        string query,
        int maxResults = 25,
        CancellationToken ct = default)
    {
        // $search requires ConsistencyLevel: eventual header
        var response = await _graph.Users.GetAsync(req =>
        {
            req.QueryParameters.Select = UserFields;
            req.QueryParameters.Search = $"\"displayName:{query}\" OR \"mail:{query}\"";
            req.QueryParameters.Top = maxResults;
            req.Headers.Add("ConsistencyLevel", "eventual");
        }, ct);

        return response?.Value?.Select(MapUser).ToList() ?? [];
    }

    public async Task<TheItemsThingLib.User> CreateUserAsync(NewUser newUser, CancellationToken ct = default)
    {
        var user = new Microsoft.Graph.Models.User
        {
            DisplayName = newUser.DisplayName,
            UserPrincipalName = newUser.UserPrincipalName,
            MailNickname = newUser.MailNickname,
            GivenName = newUser.GivenName,
            Surname = newUser.Surname,
            JobTitle = newUser.JobTitle,
            Department = newUser.Department,
            MobilePhone = newUser.MobilePhone,
            OfficeLocation = newUser.OfficeLocation,
            AccountEnabled = newUser.AccountEnabled,
            PasswordProfile = new PasswordProfile
            {
                Password = newUser.Password,
                ForceChangePasswordNextSignIn = newUser.ForceChangePasswordNextSignIn
            }
        };

        var created = await _graph.Users.PostAsync(user, cancellationToken: ct)
            ?? throw new InvalidOperationException("Graph API returned null after user creation.");

        return MapUser(created);
    }

    public async Task UpdateUserAsync(string userIdOrUpn, UpdateUser patch, CancellationToken ct = default)
    {
        var user = new Microsoft.Graph.Models.User();

        if (patch.DisplayName is not null) user.DisplayName = patch.DisplayName;
        if (patch.GivenName is not null) user.GivenName = patch.GivenName;
        if (patch.Surname is not null) user.Surname = patch.Surname;
        if (patch.JobTitle is not null) user.JobTitle = patch.JobTitle;
        if (patch.Department is not null) user.Department = patch.Department;
        if (patch.MobilePhone is not null) user.MobilePhone = patch.MobilePhone;
        if (patch.OfficeLocation is not null) user.OfficeLocation = patch.OfficeLocation;
        if (patch.AccountEnabled is not null) user.AccountEnabled = patch.AccountEnabled;

        await _graph.Users[userIdOrUpn].PatchAsync(user, cancellationToken: ct);
    }

    public async Task DisableUserAsync(string userIdOrUpn, CancellationToken ct = default)
    {
        await _graph.Users[userIdOrUpn].PatchAsync(
            new Microsoft.Graph.Models.User { AccountEnabled = false },
            cancellationToken: ct);
    }

    public async Task DeleteUserAsync(string userIdOrUpn, CancellationToken ct = default)
    {
        await _graph.Users[userIdOrUpn].DeleteAsync(cancellationToken: ct);
    }

    private static TheItemsThingLib.User MapUser(Microsoft.Graph.Models.User u) => new()
    {
        Id = u.Id ?? string.Empty,
        DisplayName = u.DisplayName,
        GivenName = u.GivenName,
        Surname = u.Surname,
        Mail = u.Mail,
        UserPrincipalName = u.UserPrincipalName,
        JobTitle = u.JobTitle,
        Department = u.Department,
        MobilePhone = u.MobilePhone,
        OfficeLocation = u.OfficeLocation,
        AccountEnabled = u.AccountEnabled ?? false
    };
}
