namespace Finitech.BuildingBlocks.SharedKernel.Primitives;

/// <summary>
/// Base class for aggregate roots with domain event support.
/// </summary>
public abstract class AggregateRoot<TId> : Entity<TId>, IAggregateRoot where TId : notnull
{
    private readonly List<DomainEvent> _domainEvents = new();

    protected AggregateRoot() : base() { }

    protected AggregateRoot(TId id) : base(id) { }

    public IReadOnlyCollection<DomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    protected void AddDomainEvent(DomainEvent eventItem)
    {
        _domainEvents.Add(eventItem);
    }

    public void RemoveDomainEvent(DomainEvent eventItem)
    {
        _domainEvents.Remove(eventItem);
    }

    public void ClearDomainEvents()
    {
        _domainEvents.Clear();
    }
}

/// <summary>
/// Aggregate root with GUID as identifier.
/// </summary>
public abstract class AggregateRoot : AggregateRoot<Guid>
{
    protected AggregateRoot() : base(Guid.NewGuid()) { }

    protected AggregateRoot(Guid id) : base(id) { }
}

public interface IAggregateRoot
{
    IReadOnlyCollection<DomainEvent> DomainEvents { get; }
    void ClearDomainEvents();
}
