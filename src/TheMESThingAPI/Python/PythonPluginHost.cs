using PySharpLib;
using PySharpLib.Runtime;

namespace TheMESThingAPI.Python;

/// <summary>
/// Runs business rules written as Python scripts (plugins/*.py) inside the API process, using the
/// PySharp interpreter (no CPython needed). Each plugin is loaded once, cached, and can be reloaded
/// from disk without restarting the host. Calls are serialized: the interpreter is shared.
/// </summary>
public sealed class PythonPluginHost(string pluginsDirectory)
{
    private readonly PyEngine _engine = new();
    private readonly Dictionary<string, PyModule> _loaded = new();
    private readonly object _lock = new();

    /// <summary>Calls <paramref name="functionName"/> in the named plugin; the result is converted to
    /// a plain JSON-serializable object graph. A Python exception surfaces as <see cref="PyRaise"/>.</summary>
    public object? Invoke(string pluginName, string functionName, params object?[] args)
    {
        lock (_lock)
        {
            var module = GetOrLoad(pluginName);
            if (!module.Dict.TryGet(functionName, out var fn))
                throw new InvalidOperationException($"Plugin '{pluginName}' has no function '{functionName}'.");

            var result = _engine.Interp.Call(fn, args.Select(ClrMarshal.ToPython).ToArray());
            return ClrMarshal.ToPlainObject(result);
        }
    }

    /// <summary>Drops the cached module: the next call re-reads the .py file from disk.</summary>
    public void Reload(string pluginName)
    {
        lock (_lock) _loaded.Remove(pluginName);
    }

    private PyModule GetOrLoad(string pluginName)
    {
        if (_loaded.TryGetValue(pluginName, out var cached)) return cached;

        if (pluginName.Any(c => !char.IsLetterOrDigit(c) && c != '_'))
            throw new FileNotFoundException($"No such plugin: '{pluginName}'.");
        var path = Path.Combine(pluginsDirectory, pluginName + ".py");
        if (!File.Exists(path)) throw new FileNotFoundException($"No such plugin: '{pluginName}'.", path);

        var module = _engine.Run(File.ReadAllText(path), path);
        _loaded[pluginName] = module;
        return module;
    }
}
