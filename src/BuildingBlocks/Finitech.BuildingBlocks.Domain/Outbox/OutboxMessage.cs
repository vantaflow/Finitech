namespace Finitech.BuildingBlocks.Domain.Outbox;

/// <summary>
/// Represents a domain event stored in the outbox table.
/// Used for reliable event delivery via the Transactional Outbox Pattern.
/// Events are written to the outbox in the same DB transaction as the domain change,
/// then published to RabbitMQ by the OutboxProcessor.
/// </summary>
public class OutboxMessage
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string EventType { get; set; } = string.Empty;
    public string Payload { get; set; } = "{}";
    public string? AggregateId { get; set; }
    public string? AggregateType { get; set; }
    public string Status { get; set; } = "Pending";
    public int RetryCount { get; set; }
    public string? LastError { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ProcessedAt { get; set; }
}
