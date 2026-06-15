using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace TheMESThingAPIClientLib.Proxies;

public abstract class ProxyBase(HttpClient http)
{
    static readonly JsonSerializerOptions Opts = new()
    {
        PropertyNameCaseInsensitive = true,
        ReferenceHandler = ReferenceHandler.IgnoreCycles,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    protected async Task<T?> GetNullableAsync<T>(string url, CancellationToken ct) where T : class
    {
        var r = await http.GetAsync(url, ct);
        if (r.StatusCode == HttpStatusCode.NotFound) return null;
        r.EnsureSuccessStatusCode();
        return await r.Content.ReadFromJsonAsync<T>(Opts, ct);
    }

    protected async Task<T> GetRequiredAsync<T>(string url, CancellationToken ct) =>
        (await http.GetFromJsonAsync<T>(url, Opts, ct))!;

    protected async Task<IReadOnlyList<T>> GetListAsync<T>(string url, CancellationToken ct) =>
        (await http.GetFromJsonAsync<IReadOnlyList<T>>(url, Opts, ct))!;

    protected async Task<TResponse> PostAsync<TRequest, TResponse>(string url, TRequest body, CancellationToken ct)
    {
        var r = await http.PostAsJsonAsync(url, body, Opts, ct);
        r.EnsureSuccessStatusCode();
        return (await r.Content.ReadFromJsonAsync<TResponse>(Opts, ct))!;
    }

    protected async Task PostVoidAsync<T>(string url, T body, CancellationToken ct)
    {
        var r = await http.PostAsJsonAsync(url, body, Opts, ct);
        r.EnsureSuccessStatusCode();
    }

    protected async Task<T?> PutNullableAsync<T>(string url, T body, CancellationToken ct) where T : class
    {
        var r = await http.PutAsJsonAsync(url, body, Opts, ct);
        if (r.StatusCode == HttpStatusCode.NotFound) return null;
        r.EnsureSuccessStatusCode();
        return await r.Content.ReadFromJsonAsync<T>(Opts, ct);
    }

    protected async Task PutVoidAsync<T>(string url, T body, CancellationToken ct)
    {
        var r = await http.PutAsJsonAsync(url, body, Opts, ct);
        r.EnsureSuccessStatusCode();
    }

    protected async Task PutVoidAsync(string url, CancellationToken ct)
    {
        var r = await http.PutAsync(url, null, ct);
        r.EnsureSuccessStatusCode();
    }

    protected async Task<bool> DeleteBoolAsync(string url, CancellationToken ct)
    {
        var r = await http.DeleteAsync(url, ct);
        if (r.StatusCode == HttpStatusCode.NotFound) return false;
        r.EnsureSuccessStatusCode();
        return true;
    }

    protected async Task DeleteVoidAsync(string url, CancellationToken ct)
    {
        var r = await http.DeleteAsync(url, ct);
        r.EnsureSuccessStatusCode();
    }

    protected async Task<byte[]?> GetBytesAsync(string url, CancellationToken ct)
    {
        var r = await http.GetAsync(url, ct);
        if (r.StatusCode == HttpStatusCode.NotFound) return null;
        r.EnsureSuccessStatusCode();
        return await r.Content.ReadAsByteArrayAsync(ct);
    }
}
