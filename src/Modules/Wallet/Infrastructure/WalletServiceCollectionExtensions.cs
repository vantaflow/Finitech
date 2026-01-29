using Finitech.Modules.Wallet.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Finitech.Modules.Wallet.Infrastructure;

public static class WalletServiceCollectionExtensions
{
    public static IServiceCollection AddWalletInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("WalletConnection")
            ?? configuration.GetConnectionString("DefaultConnection");

        services.AddDbContext<WalletDbContext>(options =>
        {
            options.UseSqlServer(connectionString, sqlOptions =>
            {
                sqlOptions.MigrationsAssembly(typeof(WalletDbContext).Assembly.FullName);
                sqlOptions.MigrationsHistoryTable("__EFMigrationsHistory", "wallet");
                sqlOptions.EnableRetryOnFailure(3, TimeSpan.FromSeconds(30), null);
            });
        });

        return services;
    }
}
