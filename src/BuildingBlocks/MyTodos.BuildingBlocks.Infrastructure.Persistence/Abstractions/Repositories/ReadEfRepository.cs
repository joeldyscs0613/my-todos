using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using MyTodos.BuildingBlocks.Application.Contracts;
using MyTodos.SharedKernel.Abstractions;

namespace MyTodos.BuildingBlocks.Infrastructure.Persistence.Abstractions.Repositories;

public abstract class ReadEfRepository<TEntity, TId, TDbContext> : IReadRepository<TEntity, TId>
    where TEntity : AggregateRoot<TId>
    where TId : IComparable
    where TDbContext : DbContext
{
    protected readonly TDbContext Context;
   
    protected ReadEfRepository(TDbContext context)
    {
        Context = context;
    }
    
    /// <summary>
    /// Retrieves a list of entities matching the optional predicate.
    /// Uses optimized query configuration for list operations.
    /// </summary>
    /// <param name="predicate">Optional filter condition. If null, returns all entities.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Read-only list of entities matching the predicate.</returns>
    public async Task<IReadOnlyList<TEntity>> GetListAsync(
        Expression<Func<TEntity, bool>>? predicate, CancellationToken ct)
    {
        var query = GetInitialQueryForList();

        if (predicate != null)
        {
            query = query.Where(predicate);
        }

        var entities = await query.ToListAsync(ct);

        return entities;
    }

    /// <summary>
    /// Retrieves a single entity by its unique identifier.
    /// Returns the fully-loaded aggregate with all related entities.
    /// </summary>
    /// <param name="id">The unique identifier of the entity.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The entity if found; otherwise null.</returns>
    public virtual async Task<TEntity?> GetByIdAsync(TId id, CancellationToken ct)
        => await GetInitialQuery().FirstOrDefaultAsync(e => e.Id.Equals(id), ct);

    /// <summary>
    /// Retrieves the first entity matching the predicate.
    /// Useful when you expect a single result or want the first match.
    /// </summary>
    /// <param name="predicate">Filter condition to match entities.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The first matching entity if found; otherwise null.</returns>
    public virtual async Task<TEntity?> GetFirstOrDefaultAsync(
        Expression<Func<TEntity, bool>> predicate, CancellationToken ct)
        => await GetInitialQuery().FirstOrDefaultAsync(predicate, ct);

    /// <summary>
    /// Checks whether any entity exists that matches the predicate.
    /// More efficient than fetching the entity when you only need to verify existence.
    /// </summary>
    /// <param name="predicate">Filter condition to test.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>True if at least one matching entity exists; otherwise false.</returns>
    public virtual async Task<bool> ExistsAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken ct)
        => await GetInitialQuery().AnyAsync(predicate, ct);
    
    /// <summary>
    /// Override to add eager loading (Include statements).
    /// </summary>
    protected virtual IQueryable<TEntity> ApplyIncludes(IQueryable<TEntity> query)
        => query;

    /// <summary>
    /// Override to add global filters (soft delete, tenant isolation).
    /// </summary>
    protected virtual IQueryable<TEntity> ApplyGlobalFilters(IQueryable<TEntity> query)
        => query;

    /// <summary>
    /// Builds the base query for read operations with no-tracking enabled and all related entities included.
    /// This ensures DDD aggregate consistency - the same fully-loaded object shape is always returned.
    /// Override ApplyIncludes() and ApplyGlobalFilters() to customize this query instead of overriding this method directly.
    /// </summary>
    /// <returns>A no-tracking queryable configured with includes and global filters, ready for detail operations.</returns>
    protected virtual IQueryable<TEntity> GetInitialQuery()
    {
        var query = Context.Set<TEntity>().AsNoTracking();
        query = ApplyIncludes(query);
        query = ApplyGlobalFilters(query);

        return query;
    }

    /// <summary>
    /// Builds a query optimized for list operations. Override this for performance tuning when loading collections.
    /// By default, returns the same query as GetInitialQuery() to maintain aggregate consistency.
    /// For large lists, consider overriding to include only the essential navigation properties.
    /// </summary>
    /// <returns>A no-tracking queryable optimized for list operations.</returns>
    protected virtual IQueryable<TEntity> GetInitialQueryForList()
        => GetInitialQuery();
}