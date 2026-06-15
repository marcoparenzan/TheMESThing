using System.Net.Http.Headers;
using System.Text.Json;
using TheMESThingAppLib;

namespace TheMESThingApp;

public sealed class PowerBIService(HttpClient http, IConfiguration config) : IPowerBIService
{
    const string PbiScope  = "https://analysis.windows.net/powerbi/api/.default";
    const string PbiApi    = "https://api.powerbi.com/v1.0/myorg";

    public IReadOnlyList<PowerBIReportDef> GetReportDefinitions() =>
        config.GetSection("PowerBI:Reports").Get<List<PowerBIReportDef>>() ?? [];

    public async Task<PowerBIEmbedConfig> GetEmbedConfigAsync(string workspaceId, string reportId)
    {
        var token               = await AcquireServicePrincipalTokenAsync();
        var (embedUrl, name)    = await GetReportMetadataAsync(token, workspaceId, reportId);
        var embedToken          = await GenerateEmbedTokenAsync(token, workspaceId, reportId);
        return new(reportId, embedUrl, embedToken, name);
    }

    // ── Private helpers ──────────────────────────────────────────────────────

    async Task<string> AcquireServicePrincipalTokenAsync()
    {
        var tenantId     = Require("PowerBI:TenantId");
        var clientId     = Require("PowerBI:ClientId");
        var clientSecret = Require("PowerBI:ClientSecret");

        var resp = await http.PostAsync(
            $"https://login.microsoftonline.com/{tenantId}/oauth2/v2.0/token",
            new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["grant_type"]    = "client_credentials",
                ["client_id"]     = clientId,
                ["client_secret"] = clientSecret,
                ["scope"]         = PbiScope
            }));

        resp.EnsureSuccessStatusCode();
        using var doc = await resp.Content.ReadFromJsonAsync<JsonDocument>();
        return doc!.RootElement.GetProperty("access_token").GetString()!;
    }

    async Task<(string EmbedUrl, string Name)> GetReportMetadataAsync(
        string token, string workspaceId, string reportId)
    {
        using var req = AuthorizedGet(token, $"{PbiApi}/groups/{workspaceId}/reports/{reportId}");
        var resp = await http.SendAsync(req);
        resp.EnsureSuccessStatusCode();
        using var doc = await resp.Content.ReadFromJsonAsync<JsonDocument>();
        var root = doc!.RootElement;
        return (root.GetProperty("embedUrl").GetString()!, root.GetProperty("name").GetString()!);
    }

    async Task<string> GenerateEmbedTokenAsync(string token, string workspaceId, string reportId)
    {
        using var req = new HttpRequestMessage(
            HttpMethod.Post,
            $"{PbiApi}/groups/{workspaceId}/reports/{reportId}/GenerateToken");
        req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        req.Content = JsonContent.Create(new { accessLevel = "View" });

        var resp = await http.SendAsync(req);
        resp.EnsureSuccessStatusCode();
        using var doc = await resp.Content.ReadFromJsonAsync<JsonDocument>();
        return doc!.RootElement.GetProperty("token").GetString()!;
    }

    static HttpRequestMessage AuthorizedGet(string token, string url)
    {
        var req = new HttpRequestMessage(HttpMethod.Get, url);
        req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return req;
    }

    string Require(string key) =>
        config[key] ?? throw new InvalidOperationException($"Configuration key '{key}' is not set.");
}
