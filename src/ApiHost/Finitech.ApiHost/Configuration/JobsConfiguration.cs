using Finitech.Modules.IdentityAccess.Infrastructure.Jobs;
using Finitech.Modules.Ledger.Infrastructure.Jobs;
using Finitech.Modules.Wallet.Infrastructure.Jobs;
using Quartz;

namespace Finitech.ApiHost.Configuration;

public static class JobsConfiguration
{
    public static IServiceCollection ConfigureJobs(this IServiceCollection services)
    {
        services.AddQuartz(q =>
        {
            // Interest Accrual Job - Daily at 2 AM
            q.AddJob<InterestAccrualJob>(j => j.WithIdentity("interest-accrual", "ledger"));
            q.AddTrigger(t => t
                .ForJob("interest-accrual", "ledger")
                .WithIdentity("interest-accrual-trigger", "ledger")
                .WithCronSchedule("0 0 2 * * ?"));

            // Scheduled Payment Job - Every 15 minutes
            q.AddJob<ScheduledPaymentJob>(j => j.WithIdentity("scheduled-payments", "wallet"));
            q.AddTrigger(t => t
                .ForJob("scheduled-payments", "wallet")
                .WithIdentity("scheduled-payments-trigger", "wallet")
                .WithCronSchedule("0 */15 * * * ?"));

            // Token Cleanup Job - Daily at 3 AM
            q.AddJob<TokenCleanupJob>(j => j.WithIdentity("token-cleanup", "identity"));
            q.AddTrigger(t => t
                .ForJob("token-cleanup", "identity")
                .WithIdentity("token-cleanup-trigger", "identity")
                .WithCronSchedule("0 0 3 * * ?"));
        });

        services.AddQuartzHostedService(q => q.WaitForJobsToComplete = true);

        return services;
    }
}
