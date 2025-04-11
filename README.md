# aks-kargo-analysisrun-logs

[![GitHub](https://img.shields.io/github/stars/ivanjosipovic/aks-kargo-analysisrun-logs?style=social)](https://github.com/IvanJosipovic/aks-kargo-analysisrun-logs)
[![Artifact Hub](https://img.shields.io/endpoint?url=https://artifacthub.io/badge/repository/aks-kargo-analysisrun-logs)](https://artifacthub.io/packages/helm/aks-kargo-analysisrun-logs/aks-kargo-analysisrun-logs)
![Downloads](https://img.shields.io/badge/dynamic/json?url=https%3A%2F%2Fraw.githubusercontent.com%2Fipitio%2Fbackage%2Frefs%2Fheads%2Findex%2FIvanJosipovic%2Faks-kargo-analysisrun-logs%2Faks-kargo-analysisrun-logs%25252Faks-kargo-analysisrun-logs.json&query=%24.downloads&label=downloads)

## What is this?

This project is an API server which implements the [Kargo AnalysisRun Log](https://docs.kargo.io/operator-guide/advanced-installation/common-configurations/#logs-from-job-metrics) API. This allows Kargo to read AnalysisRun Logs from Azure Log Analytics Workspaces.

## Features
- Read AnalysisRun Logs from Log Analytics Workspace
- Authentication
  - [Client Secret](https://learn.microsoft.com/en-us/azure/azure-monitor/logs/api/register-app-for-token?tabs=portal)
  - [Workload Identity](https://learn.microsoft.com/en-us/azure/aks/workload-identity-overview)
- AMD64 and ARM64 support

## Example
![](/docs/image.png)

## How it Works
Kargo queries this API, which in turn queries the Azure Log Analytics Workspace to retrieve container logs.

The following query is issued:

`ContainerLogV2 | where PodNamespace == '{jobNamespace}' and PodName startswith '{jobName}' and ContainerName == '{container}' | project LogMessage`

![](/docs/Diagram.png)

## Authentication to Log Analytics Workspace


### Client Secret
1. Create an [App Registration](https://learn.microsoft.com/en-us/azure/azure-monitor/logs/api/register-app-for-token?tabs=portal) in Azure Portal
2. Grant the App Registration `Reader` Role on the Log Analytics Workspace
3. Generate a Client Secret for the App Registration
4. In the aks-kargo-analysisrun-logs Helm Chart set
    ```yaml
    settings:
      # Used to connect to the Log Analytics Workspace when using Client Secret authentication
      authentication:
        # Entra Tenant Id.
        tenantId: "enter-tenant-id"

        # Entra Application Client Id.
        clientId: "enter-application-client-id"

        # Entra Application Client Secret.
        clientSecret: "enter-application-client-secret"
    ```

### Workload Identity
1. Create a [Managed Identity](https://learn.microsoft.com/en-us/azure/aks/workload-identity-deploy-cluster#create-a-managed-identity)
2. Set the [Federated credetential](https://learn.microsoft.com/en-us/azure/aks/workload-identity-deploy-cluster#create-the-federated-identity-credential) details in the Managed Identity
  - Set Subject "system:serviceaccount:{namespace}:{release-name}"
3. Grant the Managed Identity `Reader` Role on the Log Analytics Workspace
4. In the aks-kargo-analysisrun-logs Helm Chart set
    ```yaml
    serviceAccount:
      create: true
      annotations:
        azure.workload.identity/client-id: {Enter Managed Identity Client Id}

    podLabels:
      azure.workload.identity/use: "true"
    ```

## Example Configurations
### Non-sharded Kargo Example
Set the following values in the aks-kargo-analysisrun-logs Helm Chart
```yaml
settings:
  # Used to connect to the Log Analytics Workspace when using Client Secret authentication
  authentication:
    # Entra Tenant Id.
    tenantId: "enter-tenant-id"

    # Entra Application Client Id.
    clientId: "enter-application-client-id"

    # Entra Application Client Secret.
    clientSecret: "enter-application-client-secret"

  # Requests to this API must include this value in the Authorization header.
  authorizationHeader: "my-api-key"

  # For non-sharded Kargo, use 'default' and set the Azure Monitor Workspace ID.
  shards:
  - name: default
    azureMonitorWorkspaceId: "enter-azure-monitor-workspace-id"
```

#### Kargo Values
Set the following values in the Kargo Helm Chart
```yaml
api:
  ## All settings relating to the use of Argo Rollouts by the API Server.
  rollouts:
    integrationEnabled: true
    logs:
      enabled: true
      urlTemplate: "http://aks-kargo-analysisrun-logs/logs/default/${{jobNamespace}}/${{jobName}}/${{container}}"
      httpHeaders:
        Authorization: "my-api-key"
```


### Sharded Kargo
Set the following values in the aks-kargo-analysisrun-logs Helm Chart
```yaml
settings:
  # Used to connect to the Log Analytics Workspace when using Client Secret authentication
  authentication:
    # Entra Tenant Id.
    tenantId: "enter-entra-tenant-id"

    # Entra Application Client Id.
    clientId: "enter-entra-application-client-id"

    # Entra Application Client Secret.
    clientSecret: "enter-entra-application-client-secret"

  # Requests to this API must include this value in the Authorization header.
  authorizationHeader: "my-api-key"

  # For sharded Kargo, set each shard's name and corresponding Azure Monitor Workspace ID.
  shards:
  - name: development
    azureMonitorWorkspaceId: "enter-development-azure-monitor-workspace-id"
  - Name: production
    azureMonitorWorkspaceId: "enter-production-azure-monitor-workspace-id"
```

#### Kargo Values
Set the following values in the Kargo Helm Chart
```yaml
  ## All settings relating to the use of Argo Rollouts by the API Server.
  rollouts:
    integrationEnabled: true
    logs:
      enabled: true
      urlTemplate: "http://aks-kargo-analysisrun-logs/logs/${{shard}}/${{jobNamespace}}/${{jobName}}/${{container}}"
      httpHeaders:
        Authorization: "my-api-key"
```

## Installation

Download the default [Helm Values](/charts/aks-kargo-analysisrun-logs/values.yaml)

```bash
helm repo add aks-kargo-analysisrun-logs https://ivanjosipovic.github.io/aks-kargo-analysisrun-logs

helm repo update

helm inspect values aks-kargo-analysisrun-logs/aks-kargo-analysisrun-logs > values.yaml
```
Modify the settings to fit your needs

Install Helm Chart
```bash
helm install aks-kargo-analysisrun-logs aks-kargo-analysisrun-logs/aks-kargo-analysisrun-logs --create-namespace --namespace aks-kargo-analysisrun-logs -f values.yaml
```