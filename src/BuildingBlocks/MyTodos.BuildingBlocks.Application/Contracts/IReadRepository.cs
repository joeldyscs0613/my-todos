using System.Linq.Expressions;
using MyTodos.SharedKernel.Abstractions;

namespace MyTodos.BuildingBlocks.Application.Contracts;

/// <summary>
/// Read-only repository for querying aggregate roots without modifying state.
/// Returns entities with configured includes. For pagination, use IPagedListReadRepository.
/// </summary>
/// <typeparam name="TEntity">The aggregate root entity type.</typeparam>
/// <typeparam name="TId">The type of the entity's unique identifier.</typeparam>
public interface IReadRepository<TEntity, in TId>
    where TEntity : AggregateRoot<TId>
    where TId : IComparable
{
    /// <summary>
    /// Retrieves all entities matching the optional predicate with configured includes.
    /// For paginated results, use IPagedListReadRepository instead.
    /// </summary>
    /// <param name="predicate">Optional filter predicate. If null, returns all entities.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Read-only list of all matching entities.</returns>
    Task<IReadOnlyList<TEntity>> GetAllAsync(
        Expression<Func<TEntity, bool>>? predicate,
        CancellationToken ct);

    /// <summary>
    /// Retrieves entities optimized for dropdown/select list options without includes.
    /// Override to apply filtering. Map to DTOs in application layer.
    /// </summary>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Read-only list of entities suitable for UI selection controls.</returns>
    Task<IReadOnlyList<TEntity>> GetAsOptionsAsync(CancellationToken ct);

    /// <summary>
    /// Retrieves a single entity by its identifier.
    /// </summary>
    /// <param name="id">The entity identifier.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The entity if found; otherwise null.</returns>
    Task<TEntity?> GetByIdAsync(TId id, CancellationToken ct);

    /// <summary>
    /// Retrieves the first entity matching the predicate, or null if none found.
    /// </summary>
    /// <param name="predicate">Filter condition.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The first matching entity if found; otherwise null.</returns>
    Task<TEntity?> GetFirstOrDefaultAsync(
        Expression<Func<TEntity, bool>> predicate,
        CancellationToken ct);

    /// <summary>
    /// Checks whether any entity exists matching the predicate.
    /// </summary>
    /// <param name="predicate">Filter condition to test.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>True if at least one matching entity exists; otherwise false.</returns>
    Task<bool> ExistsAsync(
        Expression<Func<TEntity, bool>> predicate,
        CancellationToken ct);
}