namespace AKS.Kargo.AnalysisRun.Logs;

public interface ILogProcessor
{
    Task<IReadOnlyList<string>> GetLogs(string environmentName, string jobNamespace, string jobName, string containerName);
}