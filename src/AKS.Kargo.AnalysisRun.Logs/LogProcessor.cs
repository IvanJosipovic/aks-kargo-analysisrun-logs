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

    public async Task<IReadOnlyList<string>> GetLogs(string shardName, string jobNamespace, string jobName, string containerName)
    {
        var shard = _settings.Shards.Where(e => e.Name == shardName).FirstOrDefault();

        if (shard == null)
        {
            _logger.LogError("Shard {shardName} not found in settings.", shardName);
            return [];
        }

        var results = await _logQueryClient.QueryWorkspaceAsync<string>(
            workspaceId: shard.AzureMonitorWorkspaceId,
            query: $"ContainerLogV2 | where PodNamespace == '{jobNamespace}' and PodName == '{jobName}' and ContainerName == '{containerName}' | project LogMessage",
            QueryTimeRange.All
        );

        return results.Value;
    }
}
