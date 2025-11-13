using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using MyTodos.BuildingBlocks.Application.Contracts;
using MyTodos.SharedKernel.Abstractions;

namespace MyTodos.BuildingBlocks.Infrastructure.Persistence.Abstractions.Repositories;

/// <summary>
/// Abstract base class for Entity Framework Core read-only repositories.
/// Provides default implementations for common read operations with extensibility points.
/// </summary>
/// <typeparam name="TEntity">The aggregate root entity type.</typeparam>
/// <typeparam name="TId">The type of the entity's unique identifier.</typeparam>
/// <typeparam name="TDbContext">The EF Core DbContext type.</typeparam>
/// <remarks>
/// <para><strong>Key Features:</strong></para>
/// <list type="bullet">
/// <item><description>No-tracking queries by default for optimal read performance</description></item>
/// <item><description>Virtual methods for customizing includes, filters, and query behavior</description></item>
/// <item><description>Separate query methods for different use cases (detail vs list vs options)</description></item>
/// <item><description>DDD aggregate root support with proper eager loading</description></item>
/// </list>
/// <para><strong>Extensibility Points:</strong></para>
/// <list type="bullet">
/// <item><description>ApplyIncludes: Add eager loading for navigation properties</description></item>
/// <item><description>ApplyGlobalFilters: Add filters like soft delete, tenant isolation</description></item>
/// <item><description>GetInitialQuery: Customize base query for detail operations, includes, etc.</description></item>
/// <item><description>GetInitialQueryForList: Optimize query for list operations</description></item>
/// <item><description>GetAsOptionsAsync: Override for custom dropdown filtering/sorting</description></item>
/// </list>
/// <para><strong>Usage Pattern:</strong></para>
/// <code>
/// public class TodoReadRepository : ReadEfRepository&lt;Todo, Guid, AppDbContext&gt;
/// {
///     public TodoReadRepository(AppDbContext context) : base(context) { }
///
///     protected override IQueryable&lt;Todo&gt; ApplyIncludes(IQueryable&lt;Todo&gt; query)
///         => query.Include(t => t.Category).Include(t => t.Tags);
///
///     protected override IQueryable&lt;Todo&gt; ApplyGlobalFilters(IQueryable&lt;Todo&gt; query)
///         => query.Where(t => !t.IsDeleted);
///
///     public override async Task&lt;IReadOnlyList&lt;Todo&gt;&gt; GetAsOptionsAsync(CancellationToken ct)
///         => await Context.Todos
///             .AsNoTracking()
///             .Where(t => t.IsActive)
///             .OrderBy(t => t.Title)
///             .ToListAsync(ct);
/// }
/// </code>
/// </remarks>
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
    /// Retrieves all entities matching the optional predicate.
    /// Uses optimized query configuration for list operations with all configured includes.
    /// </summary>
    /// <param name="predicate">Optional filter condition. If null, returns all entities.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Read-only list of all entities matching the predicate.</returns>
    /// <remarks>
    /// This method returns complete entities with all configured includes (via ApplyIncludes).
    /// For dropdown/option data, use GetAsOptionsAsync instead for better performance.
    /// For paginated results, use IPagedListReadRepository.GetPagedListAsync.
    /// </remarks>
    public async Task<IReadOnlyList<TEntity>> GetAllAsync(
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
    /// Retrieves entities optimized for dropdown/select list options.
    /// Returns entities with no includes for optimal performance.
    /// </summary>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Read-only list of entities suitable for UI selection controls.</returns>
    /// <remarks>
    /// <para>This method is optimized for retrieving minimal data needed for UI dropdowns and select lists.</para>
    /// <para>By default:</para>
    /// <list type="bullet">
    /// <item><description>No navigation property includes (AsNoTracking only)</description></item>
    /// <item><description>Applies global filters (soft delete, tenant isolation)</description></item>
    /// <item><description>Returns entities sorted by a default order</description></item>
    /// </list>
    /// <para>Override this method in derived repositories to:</para>
    /// <list type="bullet">
    /// <item><description>Add custom filtering (e.g., .Where(x => x.IsActive))</description></item>
    /// <item><description>Change sort order (e.g., .OrderBy(x => x.DisplayOrder))</description></item>
    /// <item><description>Select specific fields if needed (though mapping should be done in application layer)</description></item>
    /// </list>
    /// <para>Mapping to option DTOs (Id, Name pairs) should be done in query handlers in the application layer.</para>
    /// </remarks>
    public virtual async Task<IReadOnlyList<TEntity>> GetAsOptionsAsync(CancellationToken ct)
    {
        var query = Context.Set<TEntity>()
            .AsNoTracking();

        // Apply global filters (e.g., soft delete, tenant isolation)
        query = ApplyGlobalFilters(query);

        // No includes for options - just base entity data

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