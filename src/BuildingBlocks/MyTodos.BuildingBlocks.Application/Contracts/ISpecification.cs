using MyTodos.BuildingBlocks.Application.Abstractions.Filters;

namespace MyTodos.BuildingBlocks.Application.Contracts;

/// <summary>
/// Defines the contract for specifications that encapsulate query logic including
/// filtering, searching, sorting, and pagination.
/// </summary>
/// <typeparam name="TEntity">The entity type the specification applies to.</typeparam>
/// <typeparam name="TFilter">The filter type containing query parameters.</typeparam>
public interface ISpecification<TEntity, out TFilter>
    where TFilter : Filter
{
    /// <summary>
    /// Gets the filter containing query parameters (search terms, field filters, etc.).
    /// </summary>
    TFilter Filter { get; }

    /// <summary>
    /// Gets the current page number (1-indexed).
    /// </summary>
    int PageNumber { get; }

    /// <summary>
    /// Gets the number of items per page.
    /// </summary>
    int PageSize { get; }

    /// <summary>
    /// Gets the total count of items matching the filter (before pagination).
    /// </summary>
    int TotalCount { get; }

    /// <summary>
    /// Gets the list of valid sort field names for this specification.
    /// Used for validation to ensure sort fields are valid before query execution.
    /// </summary>
    IReadOnlyList<string> ValidSortFields { get; }

    /// <summary>
    /// Applies the complete specification to the query including filtering, searching,
    /// sorting, and pagination.
    /// </summary>
    /// <param name="query">The base queryable to apply the specification to.</param>
    /// <returns>The modified queryable with all operations applied.</returns>
    /// <exception cref="BuildingBlocks.Application.Exceptions.InvalidSortFieldException">
    /// Thrown when the specified sort field is not valid for this specification.
    /// </exception>
    IQueryable<TEntity> Apply(IQueryable<TEntity> query);

    /// <summary>
    /// Applies the specification to the query excluding pagination (no Skip/Take).
    /// Used for export functionality to retrieve all filtered/sorted results.
    /// </summary>
    /// <param name="query">The base queryable to apply the specification to.</param>
    /// <returns>The modified queryable with filters and sorting applied (pagination excluded).</returns>
    /// <exception cref="BuildingBlocks.Application.Exceptions.InvalidSortFieldException">
    /// Thrown when the specified sort field is not valid for this specification.
    /// </exception>
    /// <remarks>
    /// This method applies the same filtering, searching, and sorting logic as Apply(),
    /// but skips the pagination (Skip/Take) operations. This is useful for export scenarios
    /// where all matching results need to be returned.
    /// </remarks>
    IQueryable<TEntity> ApplyWithoutPagination(IQueryable<TEntity> query);
}