using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using TheMESThing.Contracts;
using TheMESThing.Contracts.Json;

namespace TheMESThingAPIClientLib.Proxies.Iot;

/// <summary>
/// Typed telemetry ingestion. Requests are Ontly contract types, so an invalid value or a wrong
/// unit fails on the client before anything is sent. Each method returns false when the API
/// answers 404 (unknown machine).
/// </summary>
public interface ITelemetryReadingsService
{
    Task<bool> IngestAsync(MachineTemperatureReading reading, CancellationToken ct = default);
    Task<bool> IngestAsync(MachineLineSpeedReading reading, CancellationToken ct = default);
    Task<bool> IngestAsync(MachineVibrationReading reading, CancellationToken ct = default);
    Task<bool> IngestAsync(MachineCycleTimeReading reading, CancellationToken ct = default);
}

public sealed class TelemetryReadingsProxy(HttpClient http) : ITelemetryReadingsService
{
    static readonly JsonSerializerOptions Opts = new(JsonSerializerDefaults.Web)
    {
        Converters = { new ScalarJsonConverterFactory(), new QuantityJsonConverterFactory() }
    };

    public Task<bool> IngestAsync(MachineTemperatureReading reading, CancellationToken ct = default) => PostAsync("temperature", reading, ct);
    public Task<bool> IngestAsync(MachineLineSpeedReading reading, CancellationToken ct = default) => PostAsync("line-speed", reading, ct);
    public Task<bool> IngestAsync(MachineVibrationReading reading, CancellationToken ct = default) => PostAsync("vibration", reading, ct);
    public Task<bool> IngestAsync(MachineCycleTimeReading reading, CancellationToken ct = default) => PostAsync("cycle-time", reading, ct);

    async Task<bool> PostAsync<T>(string metric, T body, CancellationToken ct)
    {
        var r = await http.PostAsJsonAsync($"api/iot/telemetry/readings/{metric}", body, Opts, ct);
        if (r.StatusCode == HttpStatusCode.NotFound) return false;
        r.EnsureSuccessStatusCode();
        return true;
    }
}
