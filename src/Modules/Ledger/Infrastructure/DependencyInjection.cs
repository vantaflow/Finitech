using Finitech.BuildingBlocks.Domain.Outbox;
using Finitech.BuildingBlocks.Infrastructure.Outbox;
using Finitech.Modules.Ledger.Contracts;
using Finitech.Modules.Ledger.Infrastructure.Data;
using Finitech.Modules.Ledger.Infrastructure.Repositories;
using Finitech.Modules.Ledger.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

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

            options.UseSqlServer(connectionString, sqlOptions =>
            {
                sqlOptions.MigrationsAssembly(typeof(LedgerDbContext).Assembly.FullName);
                sqlOptions.MigrationsHistoryTable("__EFMigrationsHistory", "ledger");
                sqlOptions.EnableRetryOnFailure(
                    maxRetryCount: 5,
                    maxRetryDelay: TimeSpan.FromSeconds(30),
                    errorNumbersToAdd: null);
            });

            // Enable detailed errors in development
            if (configuration.GetValue<bool>("Logging:EnableDetailedErrors", false))
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
        services.AddSingleton<IEventPublisher, InMemoryEventPublisher>();
        services.AddHostedService<OutboxProcessorService<LedgerDbContext>>();

        return services;
    }
}
