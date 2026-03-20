using Microsoft.Extensions.Diagnostics.HealthChecks;
using Finitech.Modules.Ledger.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Finitech.ApiHost.Configuration;

public static class HealthChecksConfig
{
    public static IServiceCollection AddHealthChecksConfig(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var healthChecks = services.AddHealthChecks();

        // PostgreSQL health check
        var connectionString = configuration.GetConnectionString("DefaultConnection");
        if (!string.IsNullOrEmpty(connectionString))
        {
            healthChecks.AddNpgsql(
                connectionString,
                healthQuery: "SELECT 1;",
                name: "postgresql",
                failureStatus: HealthStatus.Degraded,
                tags: new[] { "db", "postgresql" });
        }

        // Self health check
        healthChecks.AddCheck("self", () => HealthCheckResult.Healthy(), tags: new[] { "self" });

        // Memory health check
        healthChecks.AddCheck(
            "memory",
            () =>
            {
                var allocated = GC.GetTotalMemory(forceFullCollection: false);
                var maxAllocated = configuration.GetValue<long>("HealthChecks:MaxMemoryBytes", 1024L * 1024 * 1024); // 1GB default

                if (allocated > maxAllocated)
                {
                    return HealthCheckResult.Unhealthy(
                        $"Memory usage ({allocated / 1024 / 1024}MB) exceeds threshold ({maxAllocated / 1024 / 1024}MB)");
                }

                return HealthCheckResult.Healthy(
                    $"Memory usage: {allocated / 1024 / 1024}MB");
            },
            tags: new[] { "memory" });

        return services;
    }
}
