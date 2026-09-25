using PySharpLib.Runtime;
using TheMESThingAPI.Python;

namespace TheMESThingAPI.Endpoints;

/// <summary>Endpoints whose business logic lives in Python plugins (plugins/*.py).</summary>
public static class PluginEndpoints
{
    public sealed record OeeInput(
        long PlannedProductionTimeSeconds,
        long OperatingTimeSeconds,
        long RunTimeSeconds,
        double GoodQuantity,
        double TotalQuantity);

    public static IEndpointRouteBuilder MapPluginEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapPost("/api/analytics/oee/calculate", (OeeInput input, PythonPluginHost plugins) =>
        {
            try
            {
                return Results.Json(plugins.Invoke("oee", "calculate",
                    input.PlannedProductionTimeSeconds, input.OperatingTimeSeconds, input.RunTimeSeconds,
                    input.GoodQuantity, input.TotalQuantity));
            }
            catch (PyRaise ex)
            {
                return Results.BadRequest(new { error = ex.Message });
            }
        })
        .WithTags("Analytics – OeeCalculations")
        .WithName("CalculateOee")
        .WithSummary("Compute availability, performance, quality and OEE (Python plugin oee.py)");

        app.MapPost("/api/plugins/{name}/reload", (string name, PythonPluginHost plugins) =>
        {
            plugins.Reload(name);
            return Results.Ok(new { reloaded = name });
        })
        .WithTags("Plugins")
        .WithName("ReloadPlugin")
        .WithSummary("Re-read a Python plugin from disk without restarting the API");

        return app;
    }
}
