using Microsoft.EntityFrameworkCore;
using MyTodos.BuildingBlocks.Application.Contracts;
using MyTodos.BuildingBlocks.Application.Contracts.Persistence;
using MyTodos.BuildingBlocks.Application.Contracts.Queries;
using MyTodos.SharedKernel.Abstractions;

namespace MyTodos.BuildingBlocks.Infrastructure.Persistence.Abstractions.Repositories;

/// <summary>
/// Base implementation of IWriteRepository using Entity Framework Core.
/// Follows pragmatic CQRS approach with change tracking enabled for aggregate modifications.
/// </summary>
/// <typeparam name="TEntity">The aggregate root entity type.</typeparam>
/// <typeparam name="TId">The identifier type.</typeparam>
/// <typeparam name="TDbContext">The DbContext type.</typeparam>
/// <remarks>
/// This repository uses IEntityQueryConfiguration to ensure consistent aggregate loading
/// across write operations. The configuration defines includes, joins, and filters that
/// maintain DDD aggregate boundaries and business rules.
/// </remarks>
public abstract class WriteEfRepository<TEntity, TId, TDbContext> : IWriteRepository<TEntity, TId>
    where TEntity : AggregateRoot<TId>
    where TId : IComparable
    where TDbContext : DbContext
{
    protected readonly TDbContext Context;
    protected readonly DbSet<TEntity> Set;
    protected readonly IEntityQueryConfiguration<TEntity> QueryConfiguration;

    protected WriteEfRepository(TDbContext context, IEntityQueryConfiguration<TEntity> queryConfiguration)
    {
        Context = context  ?? throw new ArgumentNullException(nameof(context));
        Set = Context.Set<TEntity>();
        QueryConfiguration = queryConfiguration  ?? throw new ArgumentNullException(nameof(queryConfiguration));;
    }

    /// <summary>
    /// Loads an aggregate root by its identifier with change tracking enabled.
    /// Applies the centralized query configuration to ensure complete aggregate loading.
    /// Returns null if the entity is not found (does not throw exception).
    /// </summary>
    /// <param name="id">The aggregate identifier.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The fully loaded aggregate root if found, otherwise null.</returns>
    /// <remarks>
    /// This method loads the aggregate with:
    /// - Change tracking enabled (for modification operations)
    /// - All includes defined in IEntityQueryConfiguration (navigation properties, manual joins)
    /// - Global filters applied (soft delete, tenant isolation, etc.)
    /// This ensures the aggregate is in a consistent state for business rule enforcement.
    /// </remarks>
    public virtual async Task<TEntity?> GetByIdAsync(TId id, CancellationToken ct)
    {
        var query = Set.AsQueryable();
        query = QueryConfiguration.ConfigureAggregate(query);
        return await query.FirstOrDefaultAsync(e => e.Id.Equals(id), ct);
    }

    /// <summary>
    /// Adds a new aggregate root to the context.
    /// </summary>
    public virtual async Task AddAsync(TEntity entity, CancellationToken ct)
    {
        await Set.AddAsync(entity, ct);
    }

    /// <summary>
    /// Adds multiple aggregate roots to the context in a batch operation.
    /// </summary>
    public virtual async Task AddRangeAsync(IEnumerable<TEntity> entities, CancellationToken ct)
    {
        await Set.AddRangeAsync(entities, ct);
    }

    /// <summary>
    /// Marks an aggregate root as modified.
    /// If the entity is not tracked, it will be attached and marked as modified.
    /// </summary>
    public virtual Task UpdateAsync(TEntity entity, CancellationToken ct)
    {
        Set.Update(entity);
        return Task.CompletedTask;
    }

    /// <summary>
    /// Marks multiple aggregate roots as modified in a batch operation.
    /// </summary>
    public virtual Task UpdateRangeAsync(IEnumerable<TEntity> entities, CancellationToken ct)
    {
        Set.UpdateRange(entities);
        return Task.CompletedTask;
    }

    /// <summary>
    /// Marks an aggregate root for deletion.
    /// If the entity is not tracked, it will be attached before removal.
    /// </summary>
    public virtual Task DeleteAsync(TEntity entity, CancellationToken ct)
    {
        Set.Remove(entity);
        return Task.CompletedTask;
    }
}