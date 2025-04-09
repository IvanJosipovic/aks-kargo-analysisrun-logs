# aks-kargo-analysisrun-logs

[![GitHub](https://img.shields.io/github/stars/ivanjosipovic/aks-kargo-analysisrun-logs?style=social)](https://github.com/IvanJosipovic/aks-kargo-analysisrun-logs)
[![Artifact Hub](https://img.shields.io/endpoint?url=https://artifacthub.io/badge/repository/aks-kargo-analysisrun-logs)](https://artifacthub.io/packages/helm/aks-kargo-analysisrun-logs/aks-kargo-analysisrun-logs)
![Downloads](https://img.shields.io/badge/dynamic/json?url=https%3A%2F%2Fraw.githubusercontent.com%2Fipitio%2Fbackage%2Frefs%2Fheads%2Findex%2FIvanJosipovic%2Faks-kargo-analysisrun-logs%2Faks-kargo-analysisrun-logs%25252Faks-kargo-analysisrun-logs.json&query=%24.downloads&label=downloads)

## What is this?

This project is an API server which implements the [Kargo AnalysisRun Log](https://docs.kargo.io/operator-guide/advanced-installation/common-configurations/#logs-from-job-metrics) API. This allows Kargo to read AnalysisRun Logs from Azure Log Analytics Workspaces.

## Features
- Read AnalysisRun Logs from Log Analytics Workspace
- Authentication
  - [Service Principle](https://learn.microsoft.com/en-us/azure/azure-monitor/logs/api/register-app-for-token?tabs=portal)
  - [Workload Identity](ttps://learn.microsoft.com/en-us/azure/azure-monitor/logs/api/register-app-for-token?tabs=portal)
- AMD64 and ARM64 support

## Installation
### Configure Helm Values

Download the default Helm Values

```bash
helm inspect values https://ivanjosipovic.github.io/aks-kargo-analysisrun-logs/aks-kargo-analysisrun-logs > values.yaml
```

Modify the settings to fit your needs

### Install Helm Chart

```bash
helm repo add aks-kargo-analysisrun-logs https://ivanjosipovic.github.io/aks-kargo-analysisrun-logs

helm repo update

helm install aks-kargo-analysisrun-logs aks-kargo-analysisrun-logs/aks-kargo-analysisrun-logs --create-namespace --namespace aks-kargo-analysisrun-logs -f values.yaml
```
