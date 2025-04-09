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
            _logger.LogError("Shard \"{shardName}\" not found in settings.", shardName);
            return [];
        }

        var results = await _logQueryClient.QueryWorkspaceAsync<string>(
            workspaceId: shard.AzureMonitorWorkspaceId,
            query: $"ContainerLogV2 | where PodNamespace == '{jobNamespace}' and PodName startswith '{jobName}' and ContainerName == '{containerName}' | project LogMessage",
            QueryTimeRange.All,
            options: new LogsQueryOptions
            {
                AllowPartialErrors = true // Prevents error, The results of this query exceed the set limit of 64000000 bytes, so not all records were returned (E_QUERY_RESULT_SET_TOO_LARGE, 0x80DA0003
            }
        );

        return results.Value;
    }
}