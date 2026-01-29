using Finitech.BuildingBlocks.Domain.Outbox;
using Finitech.BuildingBlocks.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Finitech.BuildingBlocks.Infrastructure.Outbox;

/// <summary>
/// Entity Framework Core implementation of IOutbox.
/// Stores outbox messages in the same database as the business data for atomic transactions.
/// </summary>
public class EfOutbox<TContext> : IOutbox where TContext : FinitechDbContext
{
    private readonly TContext _context;
    private readonly ILogger<EfOutbox<TContext>>? _logger;

    public EfOutbox(TContext context, ILogger<EfOutbox<TContext>>? logger = null)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _logger = logger;
    }

    public async Task AddAsync(OutboxMessage message, CancellationToken cancellationToken = default)
    {
        _context.Set<OutboxMessage>().Add(message);
        await Task.CompletedTask; // EF Core tracks changes, SaveChangesAsync will persist
    }

    public async Task<IReadOnlyList<OutboxMessage>> GetPendingMessagesAsync(int batchSize, CancellationToken cancellationToken = default)
    {
        var messages = await _context.Set<OutboxMessage>()
            .Where(m => m.Status == OutboxMessageStatus.Pending)
            .Where(m => m.RetryCount < 3) // Max retries
            .OrderBy(m => m.OccurredAt)
            .Take(batchSize)
            .ToListAsync(cancellationToken);

        // Mark as processing to prevent concurrent processing
        foreach (var message in messages)
        {
            message.Status = OutboxMessageStatus.Processing;
        }
        await _context.SaveChangesAsync(cancellationToken);

        return messages;
    }

    public async Task MarkAsProcessedAsync(Guid messageId, CancellationToken cancellationToken = default)
    {
        var message = await _context.Set<OutboxMessage>()
            .FirstOrDefaultAsync(m => m.Id == messageId, cancellationToken);

        if (message != null)
        {
            message.Status = OutboxMessageStatus.Completed;
            message.ProcessedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync(cancellationToken);

            _logger?.LogDebug("Outbox message {MessageId} marked as processed", messageId);
        }
    }

    public async Task MarkAsFailedAsync(Guid messageId, string error, CancellationToken cancellationToken = default)
    {
        var message = await _context.Set<OutboxMessage>()
            .FirstOrDefaultAsync(m => m.Id == messageId, cancellationToken);

        if (message != null)
        {
            message.Status = OutboxMessageStatus.Failed;
            message.Error = error;
            await _context.SaveChangesAsync(cancellationToken);

            _logger?.LogError("Outbox message {MessageId} marked as failed: {Error}", messageId, error);
        }
    }

    public async Task IncrementRetryAsync(Guid messageId, CancellationToken cancellationToken = default)
    {
        var message = await _context.Set<OutboxMessage>()
            .FirstOrDefaultAsync(m => m.Id == messageId, cancellationToken);

        if (message != null)
        {
            message.RetryCount++;
            // Reset to pending for next retry, unless max retries reached
            if (message.RetryCount < 3)
            {
                message.Status = OutboxMessageStatus.Pending;
            }
            else
            {
                message.Status = OutboxMessageStatus.Failed;
                message.Error = "Max retry attempts exceeded";
            }
            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}
