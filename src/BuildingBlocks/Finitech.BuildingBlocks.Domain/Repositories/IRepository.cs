using Finitech.BuildingBlocks.SharedKernel.Primitives;

namespace Finitech.BuildingBlocks.Domain.Repositories;

/// <summary>
/// Generic repository interface for aggregate roots.
/// </summary>
public interface IRepository<T> where T : IAggregateRoot
{
    Task<T?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<T>> ListAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<T>> ListAsync(ISpecification<T> spec, CancellationToken cancellationToken = default);
    Task<T> AddAsync(T entity, CancellationToken cancellationToken = default);
    Task UpdateAsync(T entity, CancellationToken cancellationToken = default);
    Task DeleteAsync(T entity, CancellationToken cancellationToken = default);
}

/// <summary>
/// Specification pattern interface for querying.
/// </summary>
public interface ISpecification<T>
{
    List<System.Linq.Expressions.Expression<Func<T, bool>>> Criteria { get; }
    List<(System.Linq.Expressions.Expression<Func<T, object>> KeySelector, bool Descending)> OrderBy { get; }
    int? Take { get; }
    int? Skip { get; }
    bool IsPagingEnabled { get; }
}
