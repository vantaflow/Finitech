namespace Finitech.BuildingBlocks.Domain.Outbox;

public interface IOutbox
{
    Task AddAsync(OutboxMessage message, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<OutboxMessage>> GetPendingMessagesAsync(int batchSize, CancellationToken cancellationToken = default);
    Task MarkAsProcessedAsync(Guid messageId, CancellationToken cancellationToken = default);
    Task MarkAsFailedAsync(Guid messageId, string error, CancellationToken cancellationToken = default);
    Task IncrementRetryAsync(Guid messageId, CancellationToken cancellationToken = default);
}
