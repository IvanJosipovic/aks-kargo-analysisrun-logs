using Azure.Monitor.Query;

namespace AKS.Kargo.AnalysisRun.Logs;

public class LogProcessor : ILogProcessor
{
    private readonly ILogger<LogProcessor> _logger;
    private readonly Settings _settings;
    private readonly LogsQueryClient _logQueryClient;

    public LogProcessor(ILogger<LogProcessor> logger, Settings settings, LogsQueryClient logQueryClient)
    {
        _logger = logger;
        _settings = settings;
        _logQueryClient = logQueryClient;
    }

    public async Task<IReadOnlyList<string>> GetLogs(string environmentName, string jobNamespace, string jobName, string containerName)
    {
        var environment = _settings.Environments.Where(e => e.Name == environmentName).FirstOrDefault();

        if (environment == null)
        {
            _logger.LogError("Environment {Environment} not found in settings.", environmentName);
            return [];
        }

        var results = await _logQueryClient.QueryWorkspaceAsync<string>(
            workspaceId: environment.AzureMonitorWorkspaceId,
            query: $"ContainerLogV2 | where PodNamespace == '{jobNamespace}' and PodName == '{jobName}' and ContainerName == '{containerName}'",
            QueryTimeRange.All
        );

        return results.Value;
    }
}
