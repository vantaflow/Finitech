using OpenTelemetry.Exporter;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace Finitech.ApiHost.Configuration;

public static class OpenTelemetryConfig
{
    public static IServiceCollection AddOpenTelemetryConfig(
        this IServiceCollection services,
        IConfiguration configuration,
        IHostEnvironment environment)
    {
        var otlpEndpoint = configuration["OTEL_EXPORTER_OTLP_ENDPOINT"];
        var serviceName = configuration["OTEL_SERVICE_NAME"] ?? "finitech-api";
        var serviceVersion = configuration["OTEL_SERVICE_VERSION"] ?? "1.0.0";

        services.AddOpenTelemetry()
            .ConfigureResource(resource => resource
                .AddService(serviceName, serviceVersion: serviceVersion)
                .AddAttributes(new Dictionary<string, object>
                {
                    ["deployment.environment"] = environment.EnvironmentName,
                    ["host.name"] = Environment.MachineName
                }))
            .WithTracing(tracing =>
            {
                tracing
                    .AddAspNetCoreInstrumentation(options =>
                    {
                        options.RecordException = true;
                        options.Filter = ctx =>
                        {
                            // Exclude health checks from tracing
                            return !ctx.Request.Path.StartsWithSegments("/health");
                        };
                    })
                    .AddHttpClientInstrumentation(options =>
                    {
                        options.RecordException = true;
                    })
                    .AddSqlClientInstrumentation(options =>
                    {
                        options.RecordException = true;
                    })
                    .AddSource("Finitech.Modules.*")
                    .SetSampler(new ParentBasedSampler(
                        new TraceIdRatioBasedSampler(
                            configuration.GetValue<double>("Tracing:SampleRate", 1.0))));

                if (!string.IsNullOrEmpty(otlpEndpoint))
                {
                    tracing.AddOtlpExporter(options =>
                    {
                        options.Endpoint = new Uri(otlpEndpoint);
                        options.Protocol = OtlpExportProtocol.Grpc;
                    });
                }
            })
            .WithMetrics(metrics =>
            {
                metrics
                    .AddAspNetCoreInstrumentation()
                    .AddHttpClientInstrumentation()
                    .AddRuntimeInstrumentation()
                    .AddProcessInstrumentation()
                    .AddPrometheusExporter();
            });

        return services;
    }
}
