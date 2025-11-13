using System.Linq.Expressions;
using MyTodos.SharedKernel.Abstractions;

namespace MyTodos.BuildingBlocks.Application.Contracts;

/// <summary>
/// Defines the contract for read-only repository operations on aggregate roots.
/// Provides methods for querying entities without modifying state.
/// </summary>
/// <typeparam name="TEntity">The aggregate root entity type.</typeparam>
/// <typeparam name="TId">The type of the entity's unique identifier.</typeparam>
/// <remarks>
/// <para><strong>Design Principles:</strong></para>
/// <list type="bullet">
/// <item><description>Read-only operations - no Create, Update, or Delete methods</description></item>
/// <item><description>Returns entities (not DTOs) - mapping happens in application layer</description></item>
/// <item><description>Supports DDD aggregate patterns - works with AggregateRoot types</description></item>
/// <item><description>Optimized query methods for different use cases (GetAll vs GetAsOptions)</description></item>
/// </list>
/// <para><strong>Use Cases:</strong></para>
/// <list type="bullet">
/// <item><description>GetAllAsync: Export functionality, bulk operations</description></item>
/// <item><description>GetAsOptionsAsync: Dropdown lists, select controls</description></item>
/// <item><description>GetByIdAsync: Detail views, single entity retrieval</description></item>
/// <item><description>GetFirstOrDefaultAsync: Lookups, unique constraint checks</description></item>
/// <item><description>ExistsAsync: Validation, duplicate checking</description></item>
/// </list>
/// <para>For paginated queries, extend with IPagedListReadRepository.</para>
/// </remarks>
public interface IReadRepository<TEntity, in TId>
    where TEntity : AggregateRoot<TId>
    where TId : IComparable
{
    /// <summary>
    /// Retrieves all entities matching the optional predicate.
    /// Returns complete entities with all configured includes.
    /// </summary>
    /// <param name="predicate">Optional filter predicate. If null, returns all entities.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Read-only list of all matching entities.</returns>
    /// <remarks>
    /// Use this method for operations that need all matching records (e.g., export functionality).
    /// For paginated results, use IPagedListReadRepository.GetPagedListAsync instead.
    /// </remarks>
    Task<IReadOnlyList<TEntity>> GetAllAsync(
        Expression<Func<TEntity, bool>>? predicate,
        CancellationToken ct);

    /// <summary>
    /// Retrieves entities optimized for dropdown/select list options.
    /// Returns minimal entity data without includes for optimal performance.
    /// </summary>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Read-only list of entities suitable for UI selection controls.</returns>
    /// <remarks>
    /// <para>This method is optimized for retrieving data for dropdowns, select lists, and other UI option controls.</para>
    /// <para>Typically returns entities with no navigation property includes to minimize data transfer.</para>
    /// <para>Override in derived repositories to apply custom filtering (e.g., IsActive = true).</para>
    /// <para>Mapping to DTOs (Id, Name pairs) should be done in the application layer query handlers.</para>
    /// </remarks>
    Task<IReadOnlyList<TEntity>> GetAsOptionsAsync(CancellationToken ct);

    /// <summary>
    /// Retrieves a single entity by its identifier.
    /// </summary>
    Task<TEntity?> GetByIdAsync(TId id, CancellationToken ct);

    /// <summary>
    /// Retrieves the first entity matching the predicate, or null if none found.
    /// </summary>
    Task<TEntity?> GetFirstOrDefaultAsync(
        Expression<Func<TEntity, bool>> predicate,
        CancellationToken ct);

    /// <summary>
    /// Checks whether any entity exists matching the predicate.
    /// </summary>
    Task<bool> ExistsAsync(
        Expression<Func<TEntity, bool>> predicate,
        CancellationToken ct);
}