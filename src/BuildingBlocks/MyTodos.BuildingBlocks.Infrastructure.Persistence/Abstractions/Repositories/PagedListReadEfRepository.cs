using Microsoft.EntityFrameworkCore;
using MyTodos.BuildingBlocks.Application.Abstractions.Filters;
using MyTodos.BuildingBlocks.Application.Contracts;
using MyTodos.SharedKernel.Abstractions;

namespace MyTodos.BuildingBlocks.Infrastructure.Persistence.Abstractions.Repositories;

/// <summary>
/// Abstract base class for Entity Framework Core repositories supporting paginated queries.
/// Extends ReadEfRepository with specification-based filtering, sorting, and pagination capabilities.
/// </summary>
/// <typeparam name="TEntity">The aggregate root entity type.</typeparam>
/// <typeparam name="TId">The type of the entity's unique identifier.</typeparam>
/// <typeparam name="TSpecification">The specification type for applying query criteria.</typeparam>
/// <typeparam name="TFilter">The filter type containing pagination and search parameters.</typeparam>
/// <typeparam name="TDbContext">The EF Core DbContext type.</typeparam>
/// <remarks>
/// <para><strong>Key Features:</strong></para>
/// <list type="bullet">
/// <item><description>Specification pattern for complex query composition</description></item>
/// <item><description>Built-in pagination, filtering, sorting, and searching</description></item>
/// <item><description>Inherits all ReadEfRepository features (includes, filters, no-tracking)</description></item>
/// <item><description>Type-safe query building with compile-time validation</description></item>
/// <item><description>Returns PagedList with metadata (total count, page info, sort info)</description></item>
/// </list>
/// <para><strong>Use Cases:</strong></para>
/// <list type="bullet">
/// <item><description>Table views with pagination, sorting, and filtering</description></item>
/// <item><description>Search results with faceted filtering</description></item>
/// <item><description>Export functionality (use GetAllAsync from base class with specification filter)</description></item>
/// <item><description>Complex queries requiring multiple filter criteria</description></item>
/// </list>
/// <para><strong>Usage Pattern:</strong></para>
/// <code>
/// public class TodoPagedReadRepository
///     : PagedListReadEfRepository&lt;Todo, Guid, TodoSpecification, TodoFilter, AppDbContext&gt;
/// {
///     public TodoPagedReadRepository(AppDbContext context) : base(context) { }
///
///     protected override IQueryable&lt;Todo&gt; ApplyIncludes(IQueryable&lt;Todo&gt; query)
///         => query
///             .Include(t => t.Category)
///             .Include(t => t.Status);
///
///     protected override IQueryable&lt;Todo&gt; ApplyGlobalFilters(IQueryable&lt;Todo&gt; query)
///         => query.Where(t => !t.IsDeleted);
/// }
///
/// // Usage in query handler:
/// var pagedResult = await _repository.GetPagedListAsync(
///     new TodoSpecification(filter),
///     cancellationToken);
/// </code>
/// <para><strong>Specification Pattern:</strong></para>
/// <para>
/// Specifications encapsulate complex query logic including filtering (Where),
/// searching (Contains), sorting (OrderBy), and pagination (Skip/Take).
/// This keeps repository focused on data access while specifications handle business query rules.
/// </para>
/// </remarks>
public abstract class PagedListReadEfRepository<TEntity, TId, TSpecification, TFilter, TDbContext>
    : ReadEfRepository<TEntity, TId, TDbContext>,
        IPagedListReadRepository<TEntity, TId, TSpecification, TFilter> where TEntity : AggregateRoot<TId>
    where TId : IComparable
    where TSpecification : class, ISpecification<TEntity, TFilter>
    where TFilter : Filter
    where TDbContext : DbContext
{

    protected PagedListReadEfRepository(TDbContext context) : base(context)
    {
    }

    /// <summary>
    /// Retrieves a paginated list of entities using the specification's filtering, sorting, and paging rules.
    /// </summary>
    /// <param name="specification">Specification containing query criteria and pagination settings.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Read-only list of entities for the requested page.</returns>
    /// <exception cref="MyTodos.BuildingBlocks.Application.Exceptions.InvalidSortFieldException">
    /// Thrown when the specification's sort field is not valid. This exception propagates to the global
    /// exception handler which maps it to a 400 Bad Request response.
    /// </exception>
    public virtual async Task<IReadOnlyList<TEntity>> GetPagedListAsync(TSpecification specification,
        CancellationToken ct)
    {
        var query = GetInitialQueryForList();

        // Apply specification (includes, filters, search, sorting, pagination)
        // InvalidSortFieldException will propagate to global handler if sort field is invalid
        query = specification.Apply(query);

        // Execute the query and return the list
        return await query.ToListAsync(ct);
    }

    /// <summary>
    /// Exports all entities matching the specification's filter and sort criteria.
    /// Returns all matching results without pagination (no Skip/Take applied).
    /// </summary>
    /// <param name="specification">Specification containing filter criteria and sort order (pagination is ignored).</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Read-only list of all entities matching the filter and sort criteria.</returns>
    /// <exception cref="MyTodos.BuildingBlocks.Application.Exceptions.InvalidSortFieldException">
    /// Thrown when the specification's sort field is not valid. This exception propagates to the global
    /// exception handler which maps it to a 400 Bad Request response.
    /// </exception>
    /// <remarks>
    /// <para>
    /// This method is designed for export functionality where users want to download
    /// all filtered/sorted results visible in a table, not just the current page.
    /// </para>
    /// <para>Example flow:</para>
    /// <list type="number">
    /// <item><description>User filters todos by category "Work" and sorts by due date</description></item>
    /// <item><description>User views paginated results (page 1 of 5)</description></item>
    /// <item><description>User clicks "Export" button</description></item>
    /// <item><description>Frontend sends same filter/sort params to export endpoint</description></item>
    /// <item><description>Backend calls ExportAsync with same specification (ignoring pagination)</description></item>
    /// <item><description>Returns all 50 "Work" todos sorted by due date</description></item>
    /// </list>
    /// <para><strong>Implementation:</strong></para>
    /// <para>
    /// Uses the specification's ApplyWithoutPagination method which applies filtering and sorting
    /// while skipping pagination. This ensures users export exactly what they see in their
    /// filtered/sorted table view.
    /// </para>
    /// <para><strong>Performance Note:</strong></para>
    /// <para>
    /// Be mindful of large result sets. Consider adding:
    /// </para>
    /// <list type="bullet">
    /// <item><description>Maximum export limits (e.g., 10,000 records max)</description></item>
    /// <item><description>Background job processing for very large exports</description></item>
    /// <item><description>Streaming responses for memory efficiency</description></item>
    /// </list>
    /// </remarks>
    public virtual async Task<IReadOnlyList<TEntity>> ExportAsync(TSpecification specification,
        CancellationToken ct)
    {
        var query = GetInitialQueryForList();

        // Apply specification filters and sorting, but skip pagination
        // InvalidSortFieldException will propagate to global handler if sort field is invalid
        query = specification.ApplyWithoutPagination(query);

        // Execute the query and return all matching results
        return await query.ToListAsync(ct);
    }
}