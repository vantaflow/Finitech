using Finitech.BuildingBlocks.Domain.Outbox;

namespace Finitech.BuildingBlocks.Infrastructure.Messaging;

/// <summary>
/// Background service that processes the outbox table.
/// Picks up unpublished events and sends them to RabbitMQ.
/// Implements the Transactional Outbox Pattern for reliable messaging.
/// </summary>
public class OutboxProcessor
{
    private readonly IEventPublisher _publisher;

    public OutboxProcessor(IEventPublisher publisher)
    {
        _publisher = publisher;
    }

    /// <summary>
    /// Process pending outbox messages.
    /// Called by a Quartz.NET job or hosted service on a timer.
    /// </summary>
    public async Task ProcessPendingAsync(IOutboxRepository repository, int batchSize = 50)
    {
        var pending = await repository.GetPendingAsync(batchSize);

        foreach (var message in pending)
        {
            try
            {
                await _publisher.PublishAsync(message);
                message.ProcessedAt = DateTime.UtcNow;
                message.Status = "Published";
                await repository.MarkAsProcessedAsync(message.Id);
            }
            catch (Exception ex)
            {
                message.RetryCount++;
                message.LastError = ex.Message;
                message.Status = message.RetryCount >= 5 ? "Failed" : "Pending";
                await repository.UpdateAsync(message);
            }
        }
    }
}

public interface IOutboxRepository
{
    Task<List<OutboxMessage>> GetPendingAsync(int batchSize);
    Task MarkAsProcessedAsync(Guid messageId);
    Task UpdateAsync(OutboxMessage message);
}
