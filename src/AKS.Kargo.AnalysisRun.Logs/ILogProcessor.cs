namespace AKS.Kargo.AnalysisRun.Logs;

public interface ILogProcessor
{
    IAsyncEnumerable<string> GetLogs(string shardName, string jobNamespace, string jobName, string containerName, CancellationToken cancellationToken);
}