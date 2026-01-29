namespace Finitech.BuildingBlocks.SharedKernel.Primitives;

/// <summary>
/// Base class for domain events.
/// </summary>
public abstract record DomainEvent
{
    public Guid EventId { get; init; } = Guid.NewGuid();
    public DateTime OccurredAt { get; init; } = DateTime.UtcNow;
    public string EventType { get; init; } = string.Empty;

    protected DomainEvent()
    {
        EventType = GetType().Name;
    }
}

/// <summary>
/// Marker interface for domain events.
/// </summary>
public interface IDomainEvent
{
    Guid EventId { get; }
    DateTime OccurredAt { get; }
    string EventType { get; }
}
