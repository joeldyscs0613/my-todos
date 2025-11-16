using MyTodos.BuildingBlocks.Application.Abstractions.Filters;
using MyTodos.BuildingBlocks.Application.Contracts.Queries;

namespace MyTodos.BuildingBlocks.Application.Contracts.Specifications;

/// <summary>
/// Defines the contract for specifications that encapsulate query logic including
/// filtering, searching, sorting, and pagination.
/// </summary>
/// <typeparam name="TEntity">The entity type the specification applies to.</typeparam>
/// <typeparam name="TFilter">The filter type containing query parameters.</typeparam>
public interface ISpecification<TEntity, out TFilter>
    where TEntity : class
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
    /// </summary>
    IReadOnlyList<string> ValidSortFields { get; }

    /// <summary>
    /// Applies filtering, searching, sorting, and pagination to the query.
    /// Uses queryConfiguration for includes by default; override for lightweight DTOs.
    /// </summary>
    /// <param name="query">The base queryable.</param>
    /// <param name="queryConfiguration">Centralized query configuration for aggregate loading.</param>
    /// <returns>The modified queryable with all operations applied.</returns>
    /// <exception cref="BuildingBlocks.Application.Exceptions.InvalidSortFieldException">
    /// Thrown when the specified sort field is invalid.
    /// </exception>
    IQueryable<TEntity> Apply(IQueryable<TEntity> query, IEntityQueryConfiguration<TEntity> queryConfiguration);

    /// <summary>
    /// Applies filtering, searching, and sorting without pagination.
    /// Used for export functionality to retrieve all filtered/sorted results.
    /// </summary>
    /// <param name="query">The base queryable.</param>
    /// <param name="queryConfiguration">Centralized query configuration for aggregate loading.</param>
    /// <returns>The modified queryable with filters and sorting applied (no pagination).</returns>
    /// <exception cref="BuildingBlocks.Application.Exceptions.InvalidSortFieldException">
    /// Thrown when the specified sort field is invalid.
    /// </exception>
    IQueryable<TEntity> ApplyWithoutPagination(IQueryable<TEntity> query, IEntityQueryConfiguration<TEntity> queryConfiguration);
}