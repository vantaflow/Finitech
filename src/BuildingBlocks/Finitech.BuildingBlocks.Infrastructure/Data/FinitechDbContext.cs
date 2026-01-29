using System.Text.Json;
using Finitech.BuildingBlocks.Domain.Outbox;
using Finitech.BuildingBlocks.Domain.Repositories;
using Finitech.BuildingBlocks.SharedKernel.Primitives;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace Finitech.BuildingBlocks.Infrastructure.Data;

public abstract class FinitechDbContext : DbContext, IUnitOfWork
{
    private readonly List<DomainEvent> _domainEvents = new();
    private IDbContextTransaction? _currentTransaction;

    protected FinitechDbContext(DbContextOptions options) : base(options)
    {
    }

    public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();

    public bool HasActiveTransaction => _currentTransaction != null;

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        // Collect domain events before saving
        var entitiesWithEvents = ChangeTracker
            .Entries<IAggregateRoot>()
            .Where(e => e.Entity.DomainEvents.Any())
            .Select(e => e.Entity)
            .ToList();

        var domainEvents = entitiesWithEvents
            .SelectMany(e => e.DomainEvents)
            .ToList();

        // Add domain events to Outbox for reliable publishing
        foreach (var domainEvent in domainEvents)
        {
            var outboxMessage = new OutboxMessage
            {
                Id = Guid.NewGuid(),
                EventType = domainEvent.EventType,
                Payload = JsonSerializer.Serialize(domainEvent, domainEvent.GetType(), new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                }),
                OccurredAt = domainEvent.OccurredAt,
                Status = OutboxMessageStatus.Pending,
                CorrelationId = GetCorrelationId()
            };

            OutboxMessages.Add(outboxMessage);
        }

        // Clear events from aggregates
        foreach (var entity in entitiesWithEvents)
        {
            entity.ClearDomainEvents();
        }

        // Update audit fields
        UpdateAuditFields();

        try
        {
            return await base.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException ex)
        {
            // Handle concurrency conflicts
            throw new ConcurrencyException("A concurrency conflict occurred.", ex);
        }
    }

    /// <summary>
    /// Gets the correlation ID from the current request context, if available.
    /// </summary>
    protected virtual string? GetCorrelationId()
    {
        // This can be overridden in derived classes to get correlation ID from HTTP context
        // or other request-scoped services
        return null;
    }

    public async Task<bool> SaveEntitiesAsync(CancellationToken cancellationToken = default)
    {
        await SaveChangesAsync(cancellationToken);
        return true;
    }

    public IReadOnlyCollection<DomainEvent> GetDomainEvents() => _domainEvents.AsReadOnly();

    public void ClearDomainEvents() => _domainEvents.Clear();

    public async Task<IDbContextTransaction?> BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_currentTransaction != null) return null;

        _currentTransaction = await Database.BeginTransactionAsync(cancellationToken);
        return _currentTransaction;
    }

    public async Task CommitTransactionAsync(IDbContextTransaction transaction, CancellationToken cancellationToken = default)
    {
        if (transaction == null) throw new ArgumentNullException(nameof(transaction));
        if (transaction != _currentTransaction) throw new InvalidOperationException("Transaction mismatch");

        try
        {
            await SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
        }
        catch
        {
            await RollbackTransactionAsync(cancellationToken);
            throw;
        }
        finally
        {
            _currentTransaction?.Dispose();
            _currentTransaction = null;
        }
    }

    public async Task RollbackTransactionAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            if (_currentTransaction != null)
            {
                await _currentTransaction.RollbackAsync(cancellationToken);
            }
        }
        finally
        {
            _currentTransaction?.Dispose();
            _currentTransaction = null;
        }
    }

    public virtual async Task<TResult> ExecuteInTransactionAsync<TResult>(
        Func<CancellationToken, Task<TResult>> operation,
        CancellationToken cancellationToken = default)
    {
        var strategy = Database.CreateExecutionStrategy();

        return await strategy.ExecuteAsync(async (ct) =>
        {
            await using var transaction = await Database.BeginTransactionAsync(ct);
            try
            {
                var result = await operation(ct);
                await transaction.CommitAsync(ct);
                return result;
            }
            catch
            {
                await transaction.RollbackAsync(ct);
                throw;
            }
        }, cancellationToken);
    }

    public virtual async Task ExecuteInTransactionAsync(
        Func<CancellationToken, Task> operation,
        CancellationToken cancellationToken = default)
    {
        var strategy = Database.CreateExecutionStrategy();

        await strategy.ExecuteAsync(async (ct) =>
        {
            await using var transaction = await Database.BeginTransactionAsync(ct);
            try
            {
                await operation(ct);
                await transaction.CommitAsync(ct);
            }
            catch
            {
                await transaction.RollbackAsync(ct);
                throw;
            }
        }, cancellationToken);
    }

    private void UpdateAuditFields()
    {
        var entries = ChangeTracker
            .Entries<Entity<Guid>>()
            .Where(e => e.State == EntityState.Added || e.State == EntityState.Modified);

        var now = DateTime.UtcNow;

        foreach (var entry in entries)
        {
            if (entry.State == EntityState.Added)
            {
                if (entry.Entity is IAuditable auditable)
                {
                    auditable.CreatedAt = now;
                }
            }

            if (entry.State == EntityState.Modified)
            {
                if (entry.Entity is IAuditable auditable)
                {
                    auditable.UpdatedAt = now;
                }
            }
        }
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Apply configurations from the current assembly
        modelBuilder.ApplyConfigurationsFromAssembly(GetType().Assembly);

        // Configure value objects to be owned (embedded)
        ConfigureValueObjects(modelBuilder);
    }

    protected virtual void ConfigureValueObjects(ModelBuilder modelBuilder)
    {
        // Override in derived contexts to configure value objects
    }
}

public interface IAuditable
{
    DateTime CreatedAt { get; set; }
    DateTime? UpdatedAt { get; set; }
}

public class ConcurrencyException : Exception
{
    public ConcurrencyException(string message, Exception innerException) : base(message, innerException)
    {
    }
}
