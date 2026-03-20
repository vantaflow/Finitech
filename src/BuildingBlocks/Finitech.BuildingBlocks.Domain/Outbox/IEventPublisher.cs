namespace Finitech.BuildingBlocks.Domain.Outbox;

public interface IEventPublisher
{
    Task PublishAsync(OutboxMessage message);
    Task PublishBatchAsync(IEnumerable<OutboxMessage> messages);
}
