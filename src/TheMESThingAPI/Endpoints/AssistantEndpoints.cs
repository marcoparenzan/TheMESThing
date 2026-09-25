using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.AI;
using TheMESThingAPI.Python;
using TheMESThingData;

namespace TheMESThingAPI.Endpoints;

/// <summary>
/// Natural-language assistant over the MES data. The chat model (any provider supported by
/// RalfAI.Providers: OpenAI, Anthropic, Azure AI Foundry) answers questions by calling tools that
/// read the API's own data and run the OEE Python plugin. Only mapped when an AI provider is configured.
/// </summary>
public static class AssistantEndpoints
{
    public sealed record ChatTurn(string Role, string Content);
    public sealed record AskRequest(string Question, IReadOnlyList<ChatTurn>? History = null);
    public sealed record AskResponse(string Answer, string Model, IReadOnlyList<string> ToolsCalled);

    const string SystemPrompt =
        "You are Ralf, the assistant of TheMESThing, a manufacturing execution system (MES). " +
        "Answer questions about machines, telemetry and OEE by calling the provided tools; never invent data. " +
        "Telemetry is stored in canonical units: Temperature in kelvin (K), LineSpeed in m/min, Vibration in m/s^2, CycleTime in s; " +
        "convert kelvin to Celsius (C = K - 273.15) when you report temperatures. " +
        "OEE for a machine is available from get_latest_oee (production counters and computed ratios): use it first; use calculate_oee only to compute what-if values from numbers the user gives you. " +
        "OEE ratios are between 0 and 1; report them as percentages. Be concise.";

    public static IEndpointRouteBuilder MapAssistantEndpoints(this IEndpointRouteBuilder app, string modelId)
    {
        app.MapPost("/api/assistant/ask", async (AskRequest request, IChatClient chat, TheMESThingDbContext db, PythonPluginHost plugins, CancellationToken ct) =>
        {
            if (string.IsNullOrWhiteSpace(request.Question))
                return Results.BadRequest(new { error = "Question is required." });

            var called = new List<string>();
            AIFunction Tool(Delegate d, string name, string description)
            {
                var inner = AIFunctionFactory.Create(d, name, description);
                return new RecordingFunction(inner, called);
            }

            var options = new ChatOptions
            {
                Tools =
                [
                    Tool(async () => (await TypedTelemetryEndpoints.LiveStatusAsync(db))
                            .Select(m => new { m.MachineId, m.MachineCode, m.MachineName, Status = m.EffectiveStatus, m.IsOnline, m.LastTelemetryAtUtc }).ToList(),
                        "list_machines", "Lists the machines (id, code, name) with their live status: Running/Idle/Down while the machine sends telemetry, Offline otherwise."),

                    Tool(async (Guid machineId) =>
                        {
                            var metrics = await db.MachineTelemetries.AsNoTracking().Where(t => t.MachineId == machineId)
                                .Select(t => t.MetricName).Distinct().Take(20).ToListAsync(ct);
                            var latest = new List<object>();
                            foreach (var metric in metrics)
                            {
                                var t = await db.MachineTelemetries.AsNoTracking()
                                    .Where(x => x.MachineId == machineId && x.MetricName == metric)
                                    .OrderByDescending(x => x.RecordedAtUtc).FirstAsync(ct);
                                latest.Add(new { t.MetricName, t.MetricValue, t.MetricUnit, t.RecordedAtUtc });
                            }
                            return latest;
                        },
                        "get_latest_telemetry", "Latest stored value of every telemetry metric of a machine, with unit and timestamp."),

                    Tool(async (Guid machineId) =>
                        {
                            var o = await db.OeeCalculations.AsNoTracking().Where(x => x.MachineId == machineId)
                                .OrderByDescending(x => x.CalculatedAtUtc).ThenByDescending(x => x.OeeCalculationId).FirstOrDefaultAsync(ct);
                            return o is null ? null : new
                            {
                                o.PeriodType, o.PeriodDate, o.CalculatedAtUtc,
                                o.PlannedProductionTimeSeconds, o.OperatingTimeSeconds, o.RunTimeSeconds,
                                o.PlannedQuantity, o.TotalQuantity, o.GoodQuantity,
                                o.Availability, o.Performance, o.Quality, o.OeeValue,
                            };
                        },
                        "get_latest_oee", "Latest production counters of a machine (planned/operating/run time in seconds, planned/total/good quantity, reference period) with the OEE already computed from them (availability, performance, quality, oee as ratios 0..1). Returns null when the machine has sent no counters."),

                    Tool((long plannedProductionTimeSeconds, long operatingTimeSeconds, long runTimeSeconds, double goodQuantity, double totalQuantity) =>
                            plugins.Invoke("oee", "calculate", plannedProductionTimeSeconds, operatingTimeSeconds, runTimeSeconds, goodQuantity, totalQuantity),
                        "calculate_oee", "Computes availability, performance, quality and OEE (ratios 0..1) with the company's OEE rule."),
                ]
            };

            try
            {
                var messages = new List<ChatMessage> { new(ChatRole.System, SystemPrompt) };
                // Previous turns give the conversation memory; only user/assistant turns are accepted from the caller.
                foreach (var turn in (request.History ?? []).TakeLast(20))
                {
                    ChatRole? role = turn.Role.Equals("assistant", StringComparison.OrdinalIgnoreCase) ? (ChatRole?)ChatRole.Assistant
                        : turn.Role.Equals("user", StringComparison.OrdinalIgnoreCase) ? ChatRole.User : null;
                    if (role is not null && !string.IsNullOrWhiteSpace(turn.Content)) messages.Add(new ChatMessage(role.Value, turn.Content));
                }
                messages.Add(new ChatMessage(ChatRole.User, request.Question));
                var response = await chat.GetResponseAsync(messages, options, ct);
                return Results.Ok(new AskResponse(response.Text, modelId, called));
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                return Results.Problem(title: "The assistant could not answer.", detail: ex.Message, statusCode: StatusCodes.Status502BadGateway);
            }
        })
        .WithTags("Assistant")
        .WithName("AskAssistant")
        .WithSummary("Ask a question about machines, telemetry and OEE in natural language");

        return app;
    }

    /// <summary>Wraps a tool so the response can report which tools the model used.</summary>
    sealed class RecordingFunction(AIFunction inner, List<string> called) : DelegatingAIFunction(inner)
    {
        protected override async ValueTask<object?> InvokeCoreAsync(AIFunctionArguments arguments, CancellationToken cancellationToken)
        {
            lock (called) called.Add(Name);
            return await base.InvokeCoreAsync(arguments, cancellationToken);
        }
    }
}
