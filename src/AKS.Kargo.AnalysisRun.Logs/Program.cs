using Azure.Core;
using Azure.Identity;
using Azure.Monitor.Query;
using FluentValidation;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Mvc;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;

namespace AKS.Kargo.AnalysisRun.Logs;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        var settings = builder.Configuration.GetSection("Settings").Get<Settings>()!;

        new SettingsValidator().ValidateAndThrow(settings);

        builder.Services.AddSingleton(settings);

        builder.Services.AddSingleton<ILogProcessor, LogProcessor>();
        builder.Services.AddSingleton<LogsQueryClient>(sp =>
        {
            var settings = sp.GetRequiredService<Settings>();

            TokenCredential tokenCredential;

            if (settings.Authentication != null && settings.Authentication.TenantId != null && settings.Authentication.ClientId != null && settings.Authentication.ClientSecret != null)
            {
                tokenCredential = new ClientSecretCredential(settings.Authentication.TenantId, settings.Authentication.ClientId, settings.Authentication.ClientSecret);
            }
            else
            {
                tokenCredential = new DefaultAzureCredential();
            }

            return new LogsQueryClient(tokenCredential);
        });

        if (settings.LogFormat == LogFormat.JSON)
        {
            builder.Logging.AddJsonConsole(options =>
            {
                options.IncludeScopes = false;
                options.TimestampFormat = "HH:mm:ss";
            });
        }

        builder.Logging.AddFilter("Default", settings.LogLevel);
        builder.Logging.AddFilter("AKS.Kargo.AnalysisRun.Logs", settings.LogLevel);
        builder.Logging.AddFilter("Microsoft.AspNetCore", settings.LogLevel);
        builder.Logging.AddFilter("Microsoft.Extensions.Diagnostics.HealthChecks", LogLevel.Warning);
        builder.Logging.AddFilter("Microsoft.Hosting.Lifetime", LogLevel.Warning);

        builder.Services
            .AddOpenTelemetry()
            .WithMetrics(metrics =>
            {
                metrics
                    .SetResourceBuilder(ResourceBuilder.CreateDefault().AddService(serviceName: "aks-kargo-analysisrun-logs"))
                    .AddRuntimeInstrumentation()
                    .AddAspNetCoreInstrumentation()
                    .AddEventCountersInstrumentation(c =>
                    {
                        c.AddEventSources(
                            "Microsoft.AspNetCore.Hosting",
                            "Microsoft-AspNetCore-Server-Kestrel",
                            "System.Net.Http",
                            "System.Net.Sockets");
                    })
                    .AddView("request-duration", new ExplicitBucketHistogramConfiguration
                    {
                        Boundaries = [0, 0.005, 0.01, 0.025, 0.05, 0.075, 0.1, 0.25, 0.5, 0.75, 1, 2.5, 5, 7.5, 10]
                    })
                    .AddMeter(
                        "Microsoft.AspNetCore.Hosting",
                        "Microsoft.AspNetCore.Server.Kestrel",
                        "aks_kargo_analysisrun_logs"
                    )
                    .AddPrometheusExporter();
            });

        builder.Services.AddMetrics();
        builder.Services.AddHealthChecks();
        builder.Services.Configure<ForwardedHeadersOptions>(options => options.ForwardedHeaders = ForwardedHeaders.All);

        var app = builder.Build();
        app.Logger.LogInformation("Starting Application");
        app.UseForwardedHeaders();
        app.MapPrometheusScrapingEndpoint();
        app.MapHealthChecks("/health");

        // Example: "http://aks-kargo-analysisrun-logs/logs/${{shard}}/${{jobNamespace}}/${{job}}/${{container}}".
        app.MapGet("/logs/{shardName}/{jobNamespace}/{job}/{container}", async (string shardName,
                                                                                  string jobNamespace,
                                                                                  string job,
                                                                                  string container,
                                                                                  [FromServices] ILogProcessor processor,
                                                                                  HttpResponse response) =>
        {
            var logs = await processor.GetLogs(shardName, jobNamespace, job, container);

            response.ContentType = "text/event-stream";
            response.Headers.CacheControl = "no-cache";
            response.Headers.Connection = "keep-alive";
            response.StatusCode = StatusCodes.Status200OK;

            foreach (var log in logs)
            {
                await response.WriteAsync(log + Environment.NewLine);
                await response.Body.FlushAsync();
            }

            return Results.Empty;
        }).WithRequestTimeout(TimeSpan.FromMinutes(2));

        app.Run();
    }
}
