using Finitech.Modules.Ledger.Domain;
using Finitech.Modules.Ledger.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Quartz;

namespace Finitech.Modules.Ledger.Infrastructure.Jobs;

/// <summary>
/// Daily job to accrue interest on savings accounts at 2 AM
/// </summary>
[DisallowConcurrentExecution]
public class InterestAccrualJob : IJob
{
    private readonly LedgerDbContext _dbContext;
    private readonly ILogger<InterestAccrualJob> _logger;

    public InterestAccrualJob(LedgerDbContext dbContext, ILogger<InterestAccrualJob> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        _logger.LogInformation("Starting interest accrual job at {Time}", DateTime.UtcNow);

        // Note: This is a simplified implementation
        // In production, you would have interest-bearing accounts configured
        // with interest rates stored in the database

        _logger.LogInformation("Interest accrual completed (placeholder implementation)");
        await Task.CompletedTask;
    }
}
