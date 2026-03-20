using Finitech.BuildingBlocks.Domain.Outbox;

namespace Finitech.BuildingBlocks.Infrastructure.Messaging;

/// <summary>
/// Processes outbox messages and publishes them to RabbitMQ.
/// Called by OutboxProcessorService on a timer.
/// </summary>
public class RabbitMqOutboxProcessor
{
    private readonly IEventPublisher _publisher;

    public RabbitMqOutboxProcessor(IEventPublisher publisher) => _publisher = publisher;

    public async Task ProcessPendingAsync(IOutbox outbox, int batchSize = 50)
    {
        var pending = await outbox.GetPendingMessagesAsync(batchSize);

        foreach (var message in pending)
        {
            try
            {
                await _publisher.PublishAsync(message);
                await outbox.MarkAsProcessedAsync(message.Id);
            }
            catch (Exception ex)
            {
                await outbox.MarkAsFailedAsync(message.Id, ex.Message);
            }
        }
    }
}
