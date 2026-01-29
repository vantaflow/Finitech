using Serilog;
using Serilog.Events;
using Serilog.Formatting.Compact;

namespace Finitech.ApiHost.Configuration;

public static class SerilogConfig
{
    public static void ConfigureSerilog(WebApplicationBuilder builder)
    {
        var environment = builder.Environment;

        var loggerConfiguration = new LoggerConfiguration()
            .MinimumLevel.Debug()
            .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
            .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
            .MinimumLevel.Override("System.Net.Http", LogEventLevel.Warning)
            .Enrich.FromLogContext()
            .Enrich.WithProperty("MachineName", Environment.MachineName)
            .Enrich.WithProperty("Environment", environment.EnvironmentName)
            .Enrich.WithProperty("Application", "Finitech.ApiHost")
            .WriteTo.Console(
                outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj} {Properties:j}{NewLine}{Exception}");

        // Structured logging in production
        if (environment.IsProduction())
        {
            loggerConfiguration.WriteTo.File(
                new CompactJsonFormatter(),
                "logs/finitech-api-.log",
                rollingInterval: RollingInterval.Day,
                retainedFileCountLimit: 30);
        }
        else
        {
            loggerConfiguration.WriteTo.Debug();
        }

        // Seq logging if configured
        var seqUrl = builder.Configuration["Seq:ServerUrl"];
        if (!string.IsNullOrEmpty(seqUrl))
        {
            loggerConfiguration.WriteTo.Seq(
                seqUrl,
                apiKey: builder.Configuration["Seq:ApiKey"],
                restrictedToMinimumLevel: LogEventLevel.Information);
        }

        Log.Logger = loggerConfiguration.CreateLogger();
        builder.Host.UseSerilog();
    }
}
