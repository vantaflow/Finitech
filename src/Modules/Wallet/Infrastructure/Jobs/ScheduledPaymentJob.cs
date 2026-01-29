using Finitech.Modules.Wallet.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Quartz;

namespace Finitech.Modules.Wallet.Infrastructure.Jobs;

/// <summary>
/// Job to execute scheduled wallet payments
/// </summary>
[DisallowConcurrentExecution]
public class ScheduledPaymentJob : IJob
{
    private readonly WalletDbContext _dbContext;
    private readonly ILogger<ScheduledPaymentJob> _logger;

    public ScheduledPaymentJob(WalletDbContext dbContext, ILogger<ScheduledPaymentJob> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        _logger.LogInformation("Starting scheduled payment job at {Time}", DateTime.UtcNow);

        var duePayments = await _dbContext.ScheduledPayments
            .Where(p => p.Status == "Active")
            .Where(p => p.NextExecutionAt <= DateTime.UtcNow)
            .ToListAsync(context.CancellationToken);

        int executedCount = 0;
        foreach (var payment in duePayments)
        {
            try
            {
                // Get wallet balance
                var balance = await _dbContext.WalletBalances
                    .FirstOrDefaultAsync(b => b.WalletId == payment.WalletId && b.CurrencyCode == payment.CurrencyCode, context.CancellationToken);

                if (balance == null || balance.AvailableBalanceMinorUnits < payment.AmountMinorUnits)
                {
                    _logger.LogWarning("Insufficient funds for scheduled payment {PaymentId}", payment.Id);
                    payment.Status = "Failed";
                    continue;
                }

                // Execute payment (simplified - in real scenario would call payment service)
                balance.BalanceMinorUnits -= payment.AmountMinorUnits;

                // Create transaction
                var transaction = new Entities.WalletTransaction
                {
                    Id = Guid.NewGuid(),
                    WalletId = payment.WalletId,
                    TransactionType = "ScheduledPayment",
                    CurrencyCode = payment.CurrencyCode,
                    AmountMinorUnits = payment.AmountMinorUnits,
                    BalanceAfterMinorUnits = balance.BalanceMinorUnits,
                    Reference = $"Scheduled-{payment.Id}",
                    Status = "Completed",
                    CreatedAt = DateTime.UtcNow
                };

                _dbContext.WalletTransactions.Add(transaction);

                // Update next execution
                payment.LastExecutedAt = DateTime.UtcNow;
                payment.NextExecutionAt = payment.Frequency switch
                {
                    "Daily" => payment.NextExecutionAt.AddDays(1),
                    "Weekly" => payment.NextExecutionAt.AddDays(7),
                    "Monthly" => payment.NextExecutionAt.AddMonths(1),
                    _ => payment.NextExecutionAt.AddMonths(1)
                };

                // Check if reached end date
                if (payment.EndDate.HasValue && payment.NextExecutionAt > payment.EndDate.Value)
                {
                    payment.Status = "Completed";
                }

                executedCount++;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to execute scheduled payment {PaymentId}", payment.Id);
            }
        }

        await _dbContext.SaveChangesAsync(context.CancellationToken);
        _logger.LogInformation("Scheduled payment job completed. Executed {Count} payments", executedCount);
    }
}
