using System.Net;
using System.Net.Http.Json;

namespace TheMESThingAPIClientLib.Proxies.Analytics;

public sealed record OeeInput(
    long PlannedProductionTimeSeconds,
    long OperatingTimeSeconds,
    long RunTimeSeconds,
    double GoodQuantity,
    double TotalQuantity);

/// <summary>Ratios are 0..1; null when they cannot be computed (a zero denominator).</summary>
public sealed record OeeResult(double? Availability, double? Performance, double? Quality, double? Oee);

/// <summary>Computes OEE through the API, which delegates the formula to the Python plugin oee.py.</summary>
public interface IOeeCalculator
{
    /// <summary>Returns the result, or an error message when the API rejects the input (HTTP 400 from the plugin).</summary>
    Task<(OeeResult? Result, string? Error)> CalculateAsync(OeeInput input, CancellationToken ct = default);
}

public sealed class OeeCalculatorProxy(HttpClient http) : IOeeCalculator
{
    public async Task<(OeeResult? Result, string? Error)> CalculateAsync(OeeInput input, CancellationToken ct = default)
    {
        var r = await http.PostAsJsonAsync("api/analytics/oee/calculate", input, ct);
        if (r.StatusCode == HttpStatusCode.BadRequest)
        {
            var body = await r.Content.ReadFromJsonAsync<ErrorBody>(ct);
            return (null, body?.Error ?? "Invalid OEE input.");
        }
        r.EnsureSuccessStatusCode();
        return (await r.Content.ReadFromJsonAsync<OeeResult>(new System.Text.Json.JsonSerializerOptions(System.Text.Json.JsonSerializerDefaults.Web), ct), null);
    }

    sealed record ErrorBody(string? Error);
}
