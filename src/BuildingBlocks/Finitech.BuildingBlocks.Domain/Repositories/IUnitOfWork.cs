using Finitech.BuildingBlocks.SharedKernel.Primitives;

namespace Finitech.BuildingBlocks.Domain.Repositories;

/// <summary>
/// Unit of Work pattern for transaction management.
/// </summary>
public interface IUnitOfWork : IDisposable
{
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Saves changes and returns true if successful. Use this for domain-driven scenarios.
    /// </summary>
    Task<bool> SaveEntitiesAsync(CancellationToken cancellationToken = default);

    Task<TResult> ExecuteInTransactionAsync<TResult>(
        Func<CancellationToken, Task<TResult>> operation,
        CancellationToken cancellationToken = default);

    Task ExecuteInTransactionAsync(
        Func<CancellationToken, Task> operation,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets domain events collected during SaveChanges.
    /// </summary>
    IReadOnlyCollection<DomainEvent> GetDomainEvents();

    /// <summary>
    /// Clears collected domain events.
    /// </summary>
    void ClearDomainEvents();
}
