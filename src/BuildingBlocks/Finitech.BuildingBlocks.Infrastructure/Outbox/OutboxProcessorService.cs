using System.Text.Json;
using Finitech.BuildingBlocks.Domain.Outbox;
using Finitech.BuildingBlocks.Infrastructure.Data;
using Finitech.BuildingBlocks.SharedKernel.Primitives;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Finitech.BuildingBlocks.Infrastructure.Outbox;

/// <summary>
/// Background service that polls the Outbox table and publishes pending messages.
/// Implements at-least-once delivery with retry logic.
/// </summary>
public class OutboxProcessorService<TContext> : BackgroundService where TContext : FinitechDbContext
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<OutboxProcessorService<TContext>> _logger;
    private readonly TimeSpan _processingInterval = TimeSpan.FromSeconds(10);
    private readonly int _batchSize = 50;

    public OutboxProcessorService(
        IServiceProvider serviceProvider,
        ILogger<OutboxProcessorService<TContext>> logger)
    {
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Outbox Processor Service started for {ContextType}", typeof(TContext).Name);

        // Wait a bit for the application to fully start
        await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessOutboxBatchAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing outbox batch");
            }

            await Task.Delay(_processingInterval, stoppingToken);
        }

        _logger.LogInformation("Outbox Processor Service stopped");
    }

    private async Task ProcessOutboxBatchAsync(CancellationToken cancellationToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var outbox = scope.ServiceProvider.GetRequiredService<IOutbox>();
        var eventPublisher = scope.ServiceProvider.GetRequiredService<IEventPublisher>();

        var pendingMessages = await outbox.GetPendingMessagesAsync(_batchSize, cancellationToken);

        if (pendingMessages.Count == 0)
        {
            return;
        }

        _logger.LogInformation("Processing {Count} outbox messages", pendingMessages.Count);

        foreach (var message in pendingMessages)
        {
            try
            {
                await ProcessMessageAsync(message, outbox, eventPublisher, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to process outbox message {MessageId}", message.Id);
                await outbox.MarkAsFailedAsync(message.Id, ex.Message, cancellationToken);
                await outbox.IncrementRetryAsync(message.Id, cancellationToken);
            }
        }
    }

    private async Task ProcessMessageAsync(
        OutboxMessage message,
        IOutbox outbox,
        IEventPublisher eventPublisher,
        CancellationToken cancellationToken)
    {
        _logger.LogDebug("Processing outbox message {MessageId} of type {EventType}",
            message.Id, message.EventType);

        try
        {
            // Deserialize the domain event
            var domainEvent = DeserializeEvent(message);

            if (domainEvent == null)
            {
                _logger.LogError("Failed to deserialize outbox message {MessageId}", message.Id);
                await outbox.MarkAsFailedAsync(message.Id, "Deserialization failed", cancellationToken);
                return;
            }

            // Publish the event
            await eventPublisher.PublishAsync(domainEvent, cancellationToken);

            // Mark as processed
            await outbox.MarkAsProcessedAsync(message.Id, cancellationToken);

            _logger.LogDebug("Successfully processed outbox message {MessageId}", message.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing outbox message {MessageId}, will retry", message.Id);
            await outbox.IncrementRetryAsync(message.Id, cancellationToken);
            throw; // Re-throw to be caught by outer handler
        }
    }

    private DomainEvent? DeserializeEvent(OutboxMessage message)
    {
        try
        {
            // Get the type from the event type name
            var eventType = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(a => a.GetTypes())
                .FirstOrDefault(t => t.Name == message.EventType && typeof(DomainEvent).IsAssignableFrom(t));

            if (eventType == null)
            {
                _logger.LogError("Could not find type for event {EventType}", message.EventType);
                return null;
            }

            var options = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };

            var domainEvent = JsonSerializer.Deserialize(message.Payload, eventType, options) as DomainEvent;
            return domainEvent;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to deserialize outbox message {MessageId}", message.Id);
            return null;
        }
    }
}
