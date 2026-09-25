using System.Net;
using System.Net.Http.Json;

namespace TheMESThingAPIClientLib.Proxies.Assistant;

public sealed record ChatTurn(string Role, string Content);
public sealed record AssistantAnswer(string Answer, string Model, IReadOnlyList<string> ToolsCalled);

/// <summary>Natural-language assistant exposed by the API (POST /api/assistant/ask).</summary>
public interface IAssistantService
{
    /// <summary>Returns the answer, or an error message (assistant not configured, provider failure, ...).</summary>
    Task<(AssistantAnswer? Answer, string? Error)> AskAsync(string question, IReadOnlyList<ChatTurn> history, CancellationToken ct = default);
}

public sealed class AssistantProxy(HttpClient http) : IAssistantService
{
    public async Task<(AssistantAnswer? Answer, string? Error)> AskAsync(string question, IReadOnlyList<ChatTurn> history, CancellationToken ct = default)
    {
        var r = await http.PostAsJsonAsync("api/assistant/ask", new { question, history }, ct);
        if (r.StatusCode is HttpStatusCode.NotFound or HttpStatusCode.MethodNotAllowed)
            return (null, "The assistant is not enabled on the API (no AIProvider configured).");
        if (!r.IsSuccessStatusCode)
        {
            var problem = await r.Content.ReadFromJsonAsync<Problem>(ct);
            return (null, problem?.Detail ?? problem?.Error ?? $"The API answered {(int)r.StatusCode}.");
        }
        return (await r.Content.ReadFromJsonAsync<AssistantAnswer>(new System.Text.Json.JsonSerializerOptions(System.Text.Json.JsonSerializerDefaults.Web), ct), null);
    }

    sealed record Problem(string? Detail, string? Error);
}
