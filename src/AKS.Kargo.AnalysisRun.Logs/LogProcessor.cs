using Azure.Monitor.Query;
using Azure.Monitor.Query.Models;

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

    public async IAsyncEnumerable<string?> GetLogs(string shardName, string jobNamespace, string jobName, string containerName)
    {
        var shard = _settings.Shards.Where(e => e.Name == shardName).FirstOrDefault();

        if (shard == null)
        {
            _logger.LogError("Shard \"{shardName}\" not found in settings.", shardName);
            yield break;
        }

        var query = $"""
            ContainerLogV2
            | where PodNamespace == "{jobNamespace}"
            | where ContainerName == "{containerName}"
            | where PodName == toscalar(
                KubePodInventory
                | where ControllerName == "{jobName}"
                | where Namespace == "{jobNamespace}"
                | top 1 by TimeGenerated desc
                | project Name
            )
            | project LogMessage
            """;

        var results = await _logQueryClient.QueryWorkspaceAsync(shard.AzureMonitorWorkspaceId, query, QueryTimeRange.All, new LogsQueryOptions()
        {
            AllowPartialErrors = true // Prevents error, The results of this query exceed the set limit of 64000000 bytes, so not all records were returned (E_QUERY_RESULT_SET_TOO_LARGE, 0x80DA0003
        });

        foreach (LogsTableRow row in results.Value.Table.Rows)
        {
            yield return row.GetString("LogMessage");
        }
    }
}