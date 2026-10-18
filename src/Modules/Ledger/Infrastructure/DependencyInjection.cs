using Finitech.BuildingBlocks.Domain.Outbox;
using Finitech.BuildingBlocks.Infrastructure.Messaging;
using Finitech.BuildingBlocks.Infrastructure.Outbox;
using Finitech.Modules.Ledger.Contracts;
using Finitech.Modules.Ledger.Infrastructure.Data;
using Finitech.Modules.Ledger.Infrastructure.Repositories;
using Finitech.Modules.Ledger.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Finitech.Modules.Ledger.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddLedgerInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // DbContext
        services.AddDbContext<LedgerDbContext>((provider, options) =>
        {
            var connectionString = configuration.GetConnectionString("LedgerDatabase")
                ?? configuration.GetConnectionString("DefaultConnection");

            options.UseNpgsql(connectionString, npgsqlOptions =>
            {
                npgsqlOptions.MigrationsAssembly(typeof(LedgerDbContext).Assembly.FullName);
                npgsqlOptions.MigrationsHistoryTable("__EFMigrationsHistory", "ledger");
                npgsqlOptions.EnableRetryOnFailure(
                    maxRetryCount: 5,
                    maxRetryDelay: TimeSpan.FromSeconds(30),
                    errorCodesToAdd: null);
            });

            // Enable detailed errors in development
            if (bool.TryParse(configuration["Logging:EnableDetailedErrors"], out var detailedErrors) && detailedErrors)
            {
                options.EnableSensitiveDataLogging();
                options.EnableDetailedErrors();
            }
        });

        // Unit of Work
        services.AddScoped<Finitech.BuildingBlocks.Domain.Repositories.IUnitOfWork>(
            provider => provider.GetRequiredService<LedgerDbContext>());

        // Repositories
        services.AddScoped<LedgerEntryRepository>();
        services.AddScoped<AccountBalanceRepository>();

        // Services
        services.AddScoped<Finitech.Modules.Ledger.Contracts.ILedgerService, LedgerService>();

        // Outbox Pattern
        services.AddScoped<IOutbox, EfOutbox<LedgerDbContext>>();
        services.TryAddSingleton(new RabbitMqSettings());
        services.AddSingleton<IEventPublisher, RabbitMqEventPublisher>();
        services.AddHostedService<OutboxProcessorService<LedgerDbContext>>();

        return services;
    }
}
