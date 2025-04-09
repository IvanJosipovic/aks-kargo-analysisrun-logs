namespace AKS.Kargo.AnalysisRun.Logs;

public class Settings
{
    public LogLevel LogLevel { get; set; }

    public LogFormat LogFormat { get; set; }

    public List<Environment> Environments { get; set; } = null!;

    public Authentication Authentication { get; set; } = null!;
}

public class Authentication
{
    public string TenantId { get; set; } = null!;
    public string ClientId { get; set; } = null!;
    public string ClientSecret { get; set; } = null!;
}

public enum LogFormat
{
    Simple,
    JSON,
}

public class Environment
{
    public string Name { get; set; } = null!;
    public string AzureMonitorWorkspaceId { get; set; } = null!;
}
