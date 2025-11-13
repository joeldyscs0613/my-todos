using MyTodos.BuildingBlocks.Application.Abstractions.Filters;
using MyTodos.SharedKernel.Abstractions;

namespace MyTodos.BuildingBlocks.Application.Contracts;

/// <summary>
/// Extends IReadRepository with paginated query capabilities using the Specification pattern.
/// Provides methods for retrieving filtered, sorted, and paginated entity collections.
/// </summary>
/// <typeparam name="TEntity">The aggregate root entity type.</typeparam>
/// <typeparam name="TId">The type of the entity's unique identifier.</typeparam>
/// <typeparam name="TSpecification">The specification type for applying query criteria.</typeparam>
/// <typeparam name="TFilter">The filter type containing pagination, sorting, and search parameters.</typeparam>
/// <remarks>
/// <para><strong>Purpose:</strong></para>
/// <para>
/// This interface extends the basic read repository with specification-based paginated queries.
/// It inherits all base read operations (GetAllAsync, GetByIdAsync, etc.) and adds pagination support.
/// </para>
/// <para><strong>Specification Pattern:</strong></para>
/// <para>
/// Specifications encapsulate complex query logic including:
/// </para>
/// <list type="bullet">
/// <item><description>Filtering: WHERE clauses based on filter properties</description></item>
/// <item><description>Searching: Text-based search across multiple fields</description></item>
/// <item><description>Sorting: ORDER BY with dynamic field selection</description></item>
/// <item><description>Pagination: SKIP/TAKE for page-based results</description></item>
/// </list>
/// <para><strong>Design Benefits:</strong></para>
/// <list type="bullet">
/// <item><description>Separation of concerns: Query logic in specifications, data access in repositories</description></item>
/// <item><description>Testability: Specifications can be unit tested independently</description></item>
/// <item><description>Reusability: Same specification can be used for paged and unpaged queries</description></item>
/// <item><description>Type safety: Compile-time validation of filters and specifications</description></item>
/// </list>
/// </remarks>
public interface IPagedListReadRepository<TEntity, in TId, in TSpecification, in TFilter>
    : IReadRepository<TEntity, TId>
    where TEntity : AggregateRoot<TId>
    where TId : IComparable
    where TSpecification : class, ISpecification<TEntity, TFilter>
    where TFilter : Filter
{
    /// <summary>
    /// Retrieves a paginated list of entities using specification-based filtering, sorting, and paging.
    /// </summary>
    /// <param name="specification">Specification containing query criteria, sort order, and pagination settings.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Read-only list of entities for the requested page.</returns>
    /// <remarks>
    /// The specification encapsulates all query logic including filters, search terms, sort field/direction,
    /// and pagination (page number, page size). The repository applies this specification to the base query
    /// (which includes configured eager loading and global filters) and returns the requested page of results.
    /// </remarks>
    Task<IReadOnlyList<TEntity>> GetPagedListAsync(TSpecification specification, CancellationToken ct);

    /// <summary>
    /// Exports all entities matching the specification's filter and sort criteria without pagination.
    /// </summary>
    /// <param name="specification">Specification containing filter criteria and sort order (pagination settings are ignored).</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Read-only list of all entities matching the filter and sort criteria.</returns>
    /// <remarks>
    /// <para>
    /// This method is designed for export scenarios where users want to download all filtered/sorted
    /// results currently visible in their table view, not just the current page.
    /// </para>
    /// <para>
    /// The specification's filtering and sorting logic is applied, but pagination is skipped.
    /// This allows users to export exactly what they see in their filtered/sorted view.
    /// </para>
    /// </remarks>
    Task<IReadOnlyList<TEntity>> ExportAsync(TSpecification specification, CancellationToken ct);
}