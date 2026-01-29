namespace Finitech.BuildingBlocks.Domain.Outbox;

/// <summary>
/// Outbox message for reliable event publishing across modules.
/// </summary>
public class OutboxMessage
{
    public Guid Id { get; set; }
    public string EventType { get; set; } = string.Empty;
    public string Payload { get; set; } = string.Empty;
    public DateTime OccurredAt { get; set; }
    public DateTime? ProcessedAt { get; set; }
    public string Status { get; set; } = OutboxMessageStatus.Pending;
    public int RetryCount { get; set; }
    public string? Error { get; set; }
    public string? CorrelationId { get; set; }
}

public static class OutboxMessageStatus
{
    public const string Pending = "Pending";
    public const string Processing = "Processing";
    public const string Completed = "Completed";
    public const string Failed = "Failed";
}

public interface IOutbox
{
    Task AddAsync(OutboxMessage message, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<OutboxMessage>> GetPendingMessagesAsync(int batchSize, CancellationToken cancellationToken = default);
    Task MarkAsProcessedAsync(Guid messageId, CancellationToken cancellationToken = default);
    Task MarkAsFailedAsync(Guid messageId, string error, CancellationToken cancellationToken = default);
    Task IncrementRetryAsync(Guid messageId, CancellationToken cancellationToken = default);
}
