using MyTodos.SharedKernel.Abstractions;

namespace MyTodos.BuildingBlocks.Application.Contracts;

/// <summary>
/// Write repository for CQRS pattern. Includes GetByIdAsync for loading aggregates with change tracking.
/// Follows pragmatic CQRS approach (eShopOnContainers pattern). For read-only queries, use IReadRepository.
/// </summary>
/// <typeparam name="TEntity">The aggregate root entity type.</typeparam>
/// <typeparam name="TId">The identifier type.</typeparam>
public interface IWriteRepository<TEntity, in TId>
    where TEntity : AggregateRoot<TId>
    where TId : IComparable
{
    /// <summary>
    /// Loads an aggregate root with change tracking for modification. Write-context operation.
    /// </summary>
    /// <param name="id">The aggregate identifier.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The aggregate root if found, otherwise null.</returns>
    Task<TEntity?> GetByIdAsync(TId id, CancellationToken ct);

    /// <summary>
    /// Adds a new aggregate root to the context.
    /// </summary>
    /// <param name="entity">The aggregate root to add.</param>
    /// <param name="ct">Cancellation token.</param>
    Task AddAsync(TEntity entity, CancellationToken ct);

    /// <summary>
    /// Adds multiple aggregate roots to the context.
    /// </summary>
    /// <param name="entities">The aggregate roots to add.</param>
    /// <param name="ct">Cancellation token.</param>
    Task AddRangeAsync(IEnumerable<TEntity> entities, CancellationToken ct);

    /// <summary>
    /// Marks an aggregate root as modified. Attaches if not tracked.
    /// </summary>
    /// <param name="entity">The aggregate root to update.</param>
    /// <param name="ct">Cancellation token.</param>
    Task UpdateAsync(TEntity entity, CancellationToken ct);

    /// <summary>
    /// Marks multiple aggregate roots as modified.
    /// </summary>
    /// <param name="entities">The aggregate roots to update.</param>
    /// <param name="ct">Cancellation token.</param>
    Task UpdateRangeAsync(IEnumerable<TEntity> entities, CancellationToken ct);

    /// <summary>
    /// Marks an aggregate root for deletion. Attaches if not tracked.
    /// </summary>
    /// <param name="entity">The aggregate root to delete.</param>
    /// <param name="ct">Cancellation token.</param>
    Task DeleteAsync(TEntity entity, CancellationToken ct);
}