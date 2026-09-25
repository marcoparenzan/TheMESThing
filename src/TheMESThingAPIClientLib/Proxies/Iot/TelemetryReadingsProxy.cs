using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using TheMESThing.Contracts;
using TheMESThing.Contracts.Json;
using TheMESThingAPIClientLib.Models;

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

    /// <summary>Latest stored value of a metric ("Temperature", "LineSpeed", "Vibration", "CycleTime"), always in
    /// the canonical unit; null when nothing has been ingested yet.</summary>
    Task<LatestReading?> GetLatestAsync(Guid machineId, string metricName, CancellationToken ct = default);
}

public sealed record LatestReading(double Value, string? Unit, DateTime RecordedAtUtc);

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

    public async Task<LatestReading?> GetLatestAsync(Guid machineId, string metricName, CancellationToken ct = default)
    {
        var page = await http.GetFromJsonAsync<PagedResult<TheMESThingData.Entities.Iot.MachineTelemetry>>(
            $"api/iot/telemetry?machineId={machineId}&metricName={Uri.EscapeDataString(metricName)}&page=1&pageSize=1", Opts, ct);
        var item = page?.Items.FirstOrDefault();
        return item is null ? null : new LatestReading(item.MetricValue, item.MetricUnit, item.RecordedAtUtc);
    }

    async Task<bool> PostAsync<T>(string metric, T body, CancellationToken ct)
    {
        var r = await http.PostAsJsonAsync($"api/iot/telemetry/readings/{metric}", body, Opts, ct);
        if (r.StatusCode == HttpStatusCode.NotFound) return false;
        r.EnsureSuccessStatusCode();
        return true;
    }
}
