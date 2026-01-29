# Outbox Pattern Documentation

## Overview

The Outbox Pattern is implemented in Finitech to ensure reliable cross-module communication with **at-least-once delivery** guarantee. It solves the problem of dual writes: when you need to save data to the database AND publish an event to a message bus, but one of these operations might fail.

## How It Works

```
┌─────────────────┐     ┌─────────────────┐     ┌─────────────────┐
│   API Request   │────▶│   SaveChanges   │────▶│   Database      │
│                 │     │                 │     │   (Atomic)      │
└─────────────────┘     └─────────────────┘     └─────────────────┘
                               │
                               │ (Same Transaction)
                               ▼
                        ┌─────────────────┐
                        │  OutboxMessages │
                        │   (Pending)     │
                        └─────────────────┘
                               │
                               │ (Background Worker)
                               ▼
                        ┌─────────────────┐
                        │  Event Publisher│
                        │  (In-Memory)    │
                        └─────────────────┘
                               │
                               ▼
                        ┌─────────────────┐
                        │  Event Handlers │
                        │  (Other Modules)│
                        └─────────────────┘
```

## Architecture

### Core Components

1. **OutboxMessage** - Entity stored in database
   - `Id`: Unique identifier
   - `EventType`: Type of domain event
   - `Payload`: JSON serialized event data
   - `Status`: Pending | Processing | Completed | Failed
   - `RetryCount`: Number of delivery attempts
   - `CorrelationId`: For distributed tracing

2. **FinitechDbContext** - Modified to auto-save events
   - Collects domain events from aggregates during `SaveChangesAsync()`
   - Serializes events to OutboxMessages
   - Same transaction = atomic consistency

3. **OutboxProcessorService** - Background worker
   - Polls OutboxMessages table every 10 seconds
   - Processes pending messages in batches (50)
   - Deserializes and publishes events
   - Handles retries with exponential backoff
   - Max 3 retries before marking as Failed

4. **IEventPublisher** - Abstraction for message bus
   - InMemoryEventPublisher (current implementation)
   - Can be replaced with RabbitMQ, Kafka, Azure Service Bus

## Usage

### 1. Creating Domain Events

Create a record inheriting from `DomainEvent`:

```csharp
public record LedgerEntryCreatedEvent : DomainEvent
{
    public Guid EntryId { get; }
    public Guid AccountId { get; }
    public string CurrencyCode { get; }
    public long AmountMinorUnits { get; }

    public LedgerEntryCreatedEvent(Guid entryId, Guid accountId, ...)
    {
        EntryId = entryId;
        AccountId = accountId;
        // ...
    }
}
```

### 2. Raising Events from Aggregates

In your aggregate root, call `AddDomainEvent()`:

```csharp
public class LedgerEntry : AggregateRoot
{
    public static LedgerEntry Create(...)
    {
        var entry = new LedgerEntry { ... };

        // This will be automatically saved to Outbox
        entry.AddDomainEvent(new LedgerEntryCreatedEvent(
            entry.Id, entry.AccountId, ...));

        return entry;
    }
}
```

### 3. Automatic Processing

When you call `SaveChangesAsync()`:

```csharp
await _context.SaveChangesAsync();
// Events are now in OutboxMessages table

// Background worker will pick them up and publish
```

## Database Schema

### OutboxMessages Table (per module)

```sql
CREATE TABLE ledger.OutboxMessages (
    Id UNIQUEIDENTIFIER PRIMARY KEY,
    EventType NVARCHAR(200) NOT NULL,
    Payload NVARCHAR(MAX) NOT NULL,
    OccurredAt DATETIME2 NOT NULL,
    ProcessedAt DATETIME2 NULL,
    Status NVARCHAR(50) NOT NULL DEFAULT 'Pending',
    RetryCount INT NOT NULL DEFAULT 0,
    Error NVARCHAR(2000) NULL,
    CorrelationId NVARCHAR(100) NULL
);

-- Indexes for performance
CREATE INDEX IX_OutboxMessages_Status_OccurredAt
    ON ledger.OutboxMessages(Status, OccurredAt);

CREATE INDEX IX_OutboxMessages_Status_RetryCount_OccurredAt
    ON ledger.OutboxMessages(Status, RetryCount, OccurredAt);
```

## Retry Policy

| Attempt | Delay | Action |
|---------|-------|--------|
| 1st | Immediate | Try to publish |
| 2nd | 10 seconds | Retry |
| 3rd | 60 seconds | Retry |
| 4th | 5 minutes | Retry |
| 5th+ | - | Mark as Failed |

Failed messages stay in the table with `Status = "Failed"` for manual investigation.

## Monitoring

### Key Metrics to Watch

1. **Pending Messages Count**
   ```sql
   SELECT COUNT(*) FROM ledger.OutboxMessages
   WHERE Status = 'Pending';
   ```

2. **Failed Messages**
   ```sql
   SELECT * FROM ledger.OutboxMessages
   WHERE Status = 'Failed' AND RetryCount >= 3;
   ```

3. **Processing Lag**
   ```sql
   SELECT MAX(DATEDIFF(second, OccurredAt, GETUTCDATE()))
   FROM ledger.OutboxMessages
   WHERE Status = 'Pending';
   ```

### Health Checks

The background service logs:
- Information: When messages are processed successfully
- Warning: When retries occur
- Error: When message processing fails

Check logs for:
```
[INF] Processing {Count} outbox messages
[INF] Successfully processed outbox message {MessageId}
[ERR] Failed to process outbox message {MessageId}
```

## Testing

### Unit Tests

```csharp
[Fact]
public void LedgerEntry_Should_Raise_Domain_Event()
{
    // Arrange
    var accountId = Guid.NewGuid();

    // Act
    var entry = LedgerEntry.Create(accountId, "MAD",
        LedgerEntryType.Credit, 1000, "Test", null, 0);

    // Assert
    var domainEvents = entry.DomainEvents;
    domainEvents.Should().ContainSingle();
    domainEvents.First().Should().BeOfType<LedgerEntryCreatedEvent>();
}
```

### Integration Tests

```csharp
[Fact]
public async Task SaveChanges_Should_Create_Outbox_Message()
{
    // Arrange
    var entry = LedgerEntry.Create(...);
    await _repository.AddAsync(entry);

    // Act
    await _unitOfWork.SaveChangesAsync();

    // Assert
    var outboxMessage = await _context.OutboxMessages
        .FirstOrDefaultAsync();
    outboxMessage.Should().NotBeNull();
    outboxMessage.Status.Should().Be("Pending");
}
```

## Configuration

### In DependencyInjection.cs

```csharp
services.AddScoped<IOutbox, EfOutbox<LedgerDbContext>>();
services.AddSingleton<IEventPublisher, InMemoryEventPublisher>();
services.AddHostedService<OutboxProcessorService<LedgerDbContext>>();
```

### Polling Interval

Modify in `OutboxProcessorService.cs`:
```csharp
private readonly TimeSpan _processingInterval = TimeSpan.FromSeconds(10);
```

### Batch Size

```csharp
private readonly int _batchSize = 50;
```

## Troubleshooting

### Messages Not Being Processed

1. Check if background service is running:
   ```
   [INF] Outbox Processor Service started for LedgerDbContext
   ```

2. Verify database connection
3. Check for exceptions in logs

### Messages Stuck in Processing Status

If a service crashes during processing, messages may stay in "Processing" status.
They will be retried after restart (the status resets to Pending on next attempt).

### High Latency

If you see delays:
1. Increase batch size
2. Reduce polling interval
3. Check database performance
4. Consider multiple background service instances (idempotent consumers)

## Future Enhancements

1. **Change Data Capture (CDC)**: Instead of polling, use SQL Server CDC
2. **Idempotent Handlers**: Ensure event handlers can safely process duplicates
3. **Dead Letter Queue**: Separate table for permanently failed messages
4. **Metrics**: Add Prometheus metrics for outbox operations
5. **Dashboard**: Build Grafana dashboard for monitoring

## Migration Guide

### Adding Outbox to a New Module

1. Add `OutboxMessages` configuration in `OnModelCreating`:
```csharp
modelBuilder.Entity<OutboxMessage>(entity =>
{
    entity.ToTable("OutboxMessages", "yourmodule");
    entity.HasKey(e => e.Id);
    // ... configure properties
});
```

2. Register services:
```csharp
services.AddScoped<IOutbox, EfOutbox<YourDbContext>>();
services.AddHostedService<OutboxProcessorService<YourDbContext>>();
```

3. Create and run EF Core migration:
```bash
dotnet ef migrations add AddOutboxMessages \
  --project src/Modules/YourModule/Infrastructure \
  --startup-project src/ApiHost/Finitech.ApiHost
```

## References

- [Microsoft: Reliable Message Processing](https://docs.microsoft.com/en-us/azure/architecture/patterns/async-request-reply)
- [Outbox Pattern by Chris Richardson](https://microservices.io/patterns/data/transactional-outbox.html)
- [CAP Theorem and Eventual Consistency](https://en.wikipedia.org/wiki/CAP_theorem)
