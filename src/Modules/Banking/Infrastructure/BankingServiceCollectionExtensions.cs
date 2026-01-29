using Finitech.Modules.Banking.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Finitech.Modules.Banking.Infrastructure;

public static class BankingServiceCollectionExtensions
{
    public static IServiceCollection AddBankingInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("BankingConnection")
            ?? configuration.GetConnectionString("DefaultConnection");

        services.AddDbContext<BankingDbContext>(options =>
        {
            options.UseSqlServer(connectionString, sqlOptions =>
            {
                sqlOptions.MigrationsAssembly(typeof(BankingDbContext).Assembly.FullName);
                sqlOptions.MigrationsHistoryTable("__EFMigrationsHistory", "banking");
                sqlOptions.EnableRetryOnFailure(3, TimeSpan.FromSeconds(30), null);
            });

            if (Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development")
            {
                options.EnableSensitiveDataLogging();
                options.EnableDetailedErrors();
            }
        });

        return services;
    }
}
