namespace TheMESThingAppLib;

public interface IPowerBIService
{
    IReadOnlyList<PowerBIReportDef> GetReportDefinitions();
    Task<PowerBIEmbedConfig> GetEmbedConfigAsync(string workspaceId, string reportId);
}

public sealed class PowerBIReportDef
{
    public string Name { get; set; } = "";
    public string WorkspaceId { get; set; } = "";
    public string ReportId { get; set; } = "";
}

public sealed record PowerBIEmbedConfig(
    string ReportId,
    string EmbedUrl,
    string EmbedToken,
    string ReportName);
