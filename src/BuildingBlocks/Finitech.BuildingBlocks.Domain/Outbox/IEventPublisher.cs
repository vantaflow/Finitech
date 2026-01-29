using Finitech.BuildingBlocks.SharedKernel.Primitives;
using Microsoft.Extensions.Logging;

namespace Finitech.BuildingBlocks.Domain.Outbox;

/// <summary>
/// Abstraction for publishing domain events to the message bus.
/// Implementations can use RabbitMQ, Kafka, Azure Service Bus, etc.
/// </summary>
public interface IEventPublisher
{
    Task PublishAsync(DomainEvent domainEvent, CancellationToken cancellationToken = default);
    Task PublishAsync(IEnumerable<DomainEvent> domainEvents, CancellationToken cancellationToken = default);
}

/// <summary>
/// In-memory implementation of IEventPublisher for local development and testing.
/// Events are dispatched to in-memory handlers within the same process.
/// </summary>
public class InMemoryEventPublisher : IEventPublisher
{
    private readonly ILogger<InMemoryEventPublisher>? _logger;
    private readonly List<IEventHandler> _handlers = new();

    public InMemoryEventPublisher(ILogger<InMemoryEventPublisher>? logger = null)
    {
        _logger = logger;
    }

    public void RegisterHandler(IEventHandler handler)
    {
        _handlers.Add(handler);
    }

    public async Task PublishAsync(DomainEvent domainEvent, CancellationToken cancellationToken = default)
    {
        _logger?.LogInformation("Publishing event {EventType} with ID {EventId}",
            domainEvent.EventType, domainEvent.EventId);

        foreach (var handler in _handlers)
        {
            if (handler.CanHandle(domainEvent))
            {
                await handler.HandleAsync(domainEvent, cancellationToken);
            }
        }
    }

    public async Task PublishAsync(IEnumerable<DomainEvent> domainEvents, CancellationToken cancellationToken = default)
    {
        foreach (var domainEvent in domainEvents)
        {
            await PublishAsync(domainEvent, cancellationToken);
        }
    }
}

/// <summary>
/// Interface for event handlers that process specific domain events.
/// </summary>
public interface IEventHandler
{
    bool CanHandle(DomainEvent domainEvent);
    Task HandleAsync(DomainEvent domainEvent, CancellationToken cancellationToken = default);
}
