using Finitech.BuildingBlocks.Domain.Repositories;
using Finitech.BuildingBlocks.Infrastructure.Data;
using Finitech.BuildingBlocks.SharedKernel.Primitives;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Finitech.BuildingBlocks.Infrastructure.Repositories;

public class Repository<TEntity, TContext> : IRepository<TEntity>
    where TEntity : AggregateRoot<Guid>
    where TContext : FinitechDbContext
{
    protected readonly TContext Context;
    protected readonly DbSet<TEntity> DbSet;

    public Repository(TContext context)
    {
        Context = context ?? throw new ArgumentNullException(nameof(context));
        DbSet = context.Set<TEntity>();
    }

    public virtual async Task<TEntity?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await DbSet.FindAsync(new object[] { id }, cancellationToken);
    }

    public virtual async Task<IReadOnlyList<TEntity>> ListAsync(CancellationToken cancellationToken = default)
    {
        return await DbSet.ToListAsync(cancellationToken);
    }

    public virtual async Task<IReadOnlyList<TEntity>> ListAsync(ISpecification<TEntity> spec, CancellationToken cancellationToken = default)
    {
        IQueryable<TEntity> query = DbSet;

        // Apply criteria
        foreach (var criteria in spec.Criteria)
        {
            query = query.Where(criteria);
        }

        // Apply ordering
        foreach (var orderBy in spec.OrderBy)
        {
            query = orderBy.Descending
                ? query.OrderByDescending(orderBy.KeySelector)
                : query.OrderBy(orderBy.KeySelector);
        }

        // Apply paging
        if (spec.IsPagingEnabled)
        {
            query = query.Skip(spec.Skip ?? 0).Take(spec.Take ?? 20);
        }

        return await query.ToListAsync(cancellationToken);
    }

    public virtual async Task<TEntity> AddAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        await DbSet.AddAsync(entity, cancellationToken);
        return entity;
    }

    public virtual Task UpdateAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        DbSet.Update(entity);
        return Task.CompletedTask;
    }

    public virtual Task DeleteAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        DbSet.Remove(entity);
        return Task.CompletedTask;
    }

    public virtual async Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await DbSet.AnyAsync(e => e.Id == id, cancellationToken);
    }

    public virtual async Task<bool> ExistsAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default)
    {
        return await DbSet.AnyAsync(predicate, cancellationToken);
    }

    public virtual async Task<int> CountAsync(CancellationToken cancellationToken = default)
    {
        return await DbSet.CountAsync(cancellationToken);
    }

    public virtual async Task<int> CountAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default)
    {
        return await DbSet.CountAsync(predicate, cancellationToken);
    }

    public IQueryable<TEntity> AsQueryable()
    {
        return DbSet.AsQueryable();
    }
}
