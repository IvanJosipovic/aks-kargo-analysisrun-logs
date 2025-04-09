namespace AKS.Kargo.AnalysisRun.Logs;

public interface ILogProcessor
{
    Task<IReadOnlyList<string>> GetLogs(string shardName, string jobNamespace, string jobName, string containerName);
}