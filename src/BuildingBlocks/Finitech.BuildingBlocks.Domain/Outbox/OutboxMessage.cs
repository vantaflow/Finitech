namespace Finitech.BuildingBlocks.Domain.Outbox;

public enum OutboxMessageStatus
{
    Pending,
    Processing,
    Completed,
    Failed
}

public class OutboxMessage
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string EventType { get; set; } = string.Empty;
    public string Payload { get; set; } = "{}";
    public string? AggregateId { get; set; }
    public string? AggregateType { get; set; }
    public OutboxMessageStatus Status { get; set; } = OutboxMessageStatus.Pending;
    public int RetryCount { get; set; }
    public string? Error { get; set; }
    public DateTime OccurredAt { get; set; } = DateTime.UtcNow;
    public string? CorrelationId { get; set; }
    public DateTime? ProcessedAt { get; set; }
}
