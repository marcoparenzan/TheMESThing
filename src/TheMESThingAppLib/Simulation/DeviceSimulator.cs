using Microsoft.Extensions.DependencyInjection;
using TheMESThing.Contracts;
using TheMESThingAPIClientLib.Proxies.Iot;

namespace TheMESThingAppLib.Simulation;

public enum SimState { Running, Idle, Down }

/// <summary>Point-in-time view of a simulated device. Counters cover the rolling window.</summary>
public sealed record DeviceSnapshot(
    Guid MachineId, string Name, SimState State,
    long PlannedSeconds, long OperatingSeconds, long RunSeconds, long GoodQuantity, long TotalQuantity,
    long ReadingsSent, string? LastError, DateTime? LastSentUtc);

/// <summary>
/// An IoT device simulator hosted in the front-end process. Every second each started device advances its
/// state machine and posts temperature, line speed, vibration (and a cycle time whenever a cycle completes)
/// plus the production counters that feed the OEE (planned/operating/run time, total/good quantity, reference
/// period) to the API as typed Ontly readings. Shared by all circuits: a device keeps running when a page is closed.
/// </summary>
public sealed class DeviceSimulator(IServiceScopeFactory scopes) : IAsyncDisposable
{
    public const int WindowSeconds = 300;

    private readonly Dictionary<Guid, SimulatedDevice> _devices = new();
    private readonly object _lock = new();
    private CancellationTokenSource? _cts;
    private Task? _loop;

    public bool IsRunning(Guid machineId) { lock (_lock) return _devices.ContainsKey(machineId); }

    public IReadOnlyList<DeviceSnapshot> Snapshots()
    {
        lock (_lock) return _devices.Values.Select(d => d.Snapshot()).ToList();
    }

    public void Start(Guid machineId, string name)
    {
        lock (_lock)
        {
            if (_devices.ContainsKey(machineId)) return;
            _devices[machineId] = new SimulatedDevice(machineId, name);
            if (_loop is null)
            {
                _cts = new CancellationTokenSource();
                var token = _cts.Token;
                _loop = Task.Run(() => RunAsync(token));
            }
        }
    }

    public void Stop(Guid machineId)
    {
        CancellationTokenSource? cts = null;
        lock (_lock)
        {
            if (!_devices.Remove(machineId)) return;
            _ = ReportOfflineAsync(machineId);
            if (_devices.Count == 0) { cts = _cts; _cts = null; _loop = null; }
        }
        cts?.Cancel();
    }

    /// <summary>A stopped device tells the API it is offline, so its status no longer shows the last simulated state.</summary>
    private async Task ReportOfflineAsync(Guid machineId)
    {
        try
        {
            using var scope = scopes.CreateScope();
            var api = scope.ServiceProvider.GetRequiredService<ITelemetryReadingsService>();
            await api.ReportStatusAsync(new MachineStatusReport(new MachineId(machineId), new MachineStatus("Offline"), new EventTimestamp(DateTime.UtcNow)));
        }
        catch { /* best effort: the API also derives Offline from missing telemetry */ }
    }

    private async Task RunAsync(CancellationToken ct)
    {
        using var timer = new PeriodicTimer(TimeSpan.FromSeconds(1));
        try
        {
            while (await timer.WaitForNextTickAsync(ct))
            {
                SimulatedDevice[] devices;
                lock (_lock) devices = _devices.Values.ToArray();
                await Task.WhenAll(devices.Select(d => SendAsync(d, ct)));
            }
        }
        catch (OperationCanceledException) { }
    }

    private async Task SendAsync(SimulatedDevice device, CancellationToken ct)
    {
        var tick = device.Tick();
        try
        {
            using var scope = scopes.CreateScope();
            var api = scope.ServiceProvider.GetRequiredService<ITelemetryReadingsService>();
            var machine = new MachineId(device.MachineId);
            var now = new EventTimestamp(DateTime.UtcNow);

            var posts = new List<Task<bool>>
            {
                api.IngestAsync(new MachineTemperatureReading(machine,
                    TemperatureMeasure<double>.From(tick.TemperatureC, TemperatureMeasureUnits.Celsius<double>()), now), ct),
                api.IngestAsync(new MachineLineSpeedReading(machine, new LineSpeedMeasure<double>(tick.LineSpeedMetresPerMinute), now), ct),
                api.IngestAsync(new MachineVibrationReading(machine, new VibrationMeasure<double>(tick.Vibration), now), ct),
            };
            if (tick.CompletedCycleSeconds is { } cycle)
                posts.Add(api.IngestAsync(new MachineCycleTimeReading(machine, new CycleTimeMeasure<double>(cycle), now), ct));

            // Production counters over the rolling reference period: the inputs of the OEE calculation.
            var counters = device.Snapshot();
            posts.Add(api.IngestAsync(new MachineProductionCounters(machine,
                new ReferencePeriodType($"Rolling{WindowSeconds}s"), now,
                new DurationMeasure<double>(counters.PlannedSeconds),
                new DurationMeasure<double>(counters.OperatingSeconds),
                new DurationMeasure<double>(counters.RunSeconds),
                new PieceCount(counters.PlannedSeconds / SimulatedDevice.IdealCycleSeconds),
                new PieceCount(counters.TotalQuantity),
                new PieceCount(counters.GoodQuantity)), ct));

            if (counters.State != device.ReportedState)
                posts.Add(ReportStatusAsync(api, device, machine, counters.State, now, ct));

            var results = await Task.WhenAll(posts);
            device.RecordSent(results.Count(ok => ok), results.Any(ok => !ok) ? "Machine not found on the API." : null);
        }
        catch (OperationCanceledException) { throw; }
        catch (Exception ex)
        {
            device.RecordSent(0, ex.Message);
        }
    }

    private static async Task<bool> ReportStatusAsync(ITelemetryReadingsService api, SimulatedDevice device, MachineId machine, SimState state, EventTimestamp at, CancellationToken ct)
    {
        var ok = await api.ReportStatusAsync(new MachineStatusReport(machine, new MachineStatus(state.ToString()), at), ct);
        if (ok) device.ReportedState = state;
        return ok;
    }

    public ValueTask DisposeAsync()
    {
        CancellationTokenSource? cts;
        lock (_lock) { cts = _cts; _cts = null; _loop = null; _devices.Clear(); }
        cts?.Cancel();
        return ValueTask.CompletedTask;
    }
}

internal readonly record struct Tick(double TemperatureC, double LineSpeedMetresPerMinute, double Vibration, double? CompletedCycleSeconds);

/// <summary>One simulated production machine: a Running/Idle/Down state machine plus a cycle counter.</summary>
internal sealed class SimulatedDevice(Guid machineId, string name)
{
    public const double IdealCycleSeconds = 12;
    private readonly record struct Sample(int Operating, double Run, int Total, int Good);

    private readonly Random _rng = new();
    private readonly Queue<Sample> _window = new();
    private readonly object _lock = new();
    private SimState _state = SimState.Running;
    private double _temperatureC = 25;
    private double _cycleElapsed;
    private double _cycleTarget = IdealCycleSeconds;
    private long _sent;
    private string? _error;
    private DateTime? _lastSentUtc;

    public Guid MachineId { get; } = machineId;
    public SimState? ReportedState { get; set; }

    public Tick Tick()
    {
        lock (_lock)
        {
            _state = NextState(_state);

            var targetTemp = _state switch { SimState.Running => 70.0, SimState.Idle => 40.0, _ => 25.0 };
            _temperatureC += (targetTemp - _temperatureC) * 0.1 + (_rng.NextDouble() - 0.5) * 1.2;

            var speed = _state == SimState.Running ? 120 + (_rng.NextDouble() - 0.5) * 10 : 0;
            var vibration = _state == SimState.Running ? 2.0 + (_rng.NextDouble() - 0.5) * 0.8 : 0.1;
            if (_state == SimState.Running && _rng.NextDouble() < 0.02) vibration += 3 + _rng.NextDouble() * 2; // occasional spike

            double? completed = null;
            int total = 0, good = 0;
            if (_state == SimState.Running && ++_cycleElapsed >= _cycleTarget)
            {
                completed = _cycleTarget;
                total = 1;
                good = _rng.NextDouble() < 0.96 ? 1 : 0;
                _cycleElapsed = 0;
                _cycleTarget = IdealCycleSeconds + 1 + (_rng.NextDouble() - 0.5) * 2;
            }

            _window.Enqueue(new Sample(_state == SimState.Down ? 0 : 1, total * IdealCycleSeconds, total, good));
            while (_window.Count > DeviceSimulator.WindowSeconds) _window.Dequeue();

            return new Tick(_temperatureC, speed, vibration, completed);
        }
    }

    private SimState NextState(SimState state)
    {
        var roll = _rng.NextDouble();
        return state switch
        {
            SimState.Running => roll < 0.010 ? SimState.Down : roll < 0.030 ? SimState.Idle : state,
            SimState.Idle => roll < 0.20 ? SimState.Running : state,
            _ => roll < 0.10 ? SimState.Running : state,
        };
    }

    public void RecordSent(int count, string? error)
    {
        lock (_lock) { _sent += count; _error = error; _lastSentUtc = DateTime.UtcNow; }
    }

    public DeviceSnapshot Snapshot()
    {
        lock (_lock)
        {
            var operating = _window.Sum(s => s.Operating);
            var run = Math.Min(_window.Sum(s => s.Run), operating);
            return new DeviceSnapshot(MachineId, name, _state,
                _window.Count, operating, (long)Math.Round(run),
                _window.Sum(s => s.Good), _window.Sum(s => s.Total), _sent, _error, _lastSentUtc);
        }
    }
}
