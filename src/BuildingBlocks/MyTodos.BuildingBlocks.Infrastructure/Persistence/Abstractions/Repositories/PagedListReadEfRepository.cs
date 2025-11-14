using Microsoft.EntityFrameworkCore;
using MyTodos.BuildingBlocks.Application.Abstractions.Filters;
using MyTodos.BuildingBlocks.Application.Abstractions.Queries;
using MyTodos.BuildingBlocks.Application.Contracts;
using MyTodos.SharedKernel.Abstractions;

namespace MyTodos.BuildingBlocks.Infrastructure.Persistence.Abstractions.Repositories;

/// <summary>
/// Base class for EF Core repositories supporting paginated queries with specification pattern.
/// Provides filtering, sorting, pagination, and export functionality.
/// </summary>
public abstract class PagedListReadEfRepository<TEntity, TId, TSpecification, TFilter, TDbContext>
    : ReadEfRepository<TEntity, TId, TDbContext>,
        IPagedListReadRepository<TEntity, TId, TSpecification, TFilter> where TEntity : AggregateRoot<TId>
    where TId : IComparable
    where TSpecification : class, ISpecification<TEntity, TFilter>
    where TFilter : Filter
    where TDbContext : DbContext
{
    protected PagedListReadEfRepository(
        TDbContext context,
        IEntityQueryConfiguration<TEntity> queryConfiguration)
        : base(context, queryConfiguration)
    {
    }

    /// <summary>
    /// Retrieves a paginated list of entities using the specification's filtering, sorting, and paging rules.
    /// Passes the centralized query configuration to the specification for aggregate loading control.
    /// </summary>
    /// <param name="specification">Specification containing query criteria and pagination settings.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Read-only list of entities for the requested page.</returns>
    /// <exception cref="MyTodos.BuildingBlocks.Application.Exceptions.InvalidSortFieldException">
    /// Thrown when the specification's sort field is not valid. This exception propagates to the global
    /// exception handler which maps it to a 400 Bad Request response.
    /// </exception>
    /// <remarks>
    /// The specification receives IEntityQueryConfiguration and can choose to:
    /// 1. Use the full aggregate configuration (default behavior)
    /// 2. Override with custom lightweight includes for specific DTOs
    /// </remarks>
    public virtual async Task<IReadOnlyList<TEntity>> GetPagedListAsync(TSpecification specification,
        CancellationToken ct)
    {
        var query = GetInitialQueryForList();

        // Apply specification (includes, filters, search, sorting, pagination)
        // Specification receives QueryConfiguration and decides whether to use it or override
        // InvalidSortFieldException will propagate to global handler if sort field is invalid
        query = specification.Apply(query, QueryConfiguration);

        // Execute the query and return the list
        return await query.ToListAsync(ct);
    }

    /// <summary>
    /// Exports all entities matching the specification's filter and sort criteria without pagination.
    /// Use for export functionality where all filtered/sorted results are needed.
    /// </summary>
    public virtual async Task<IReadOnlyList<TEntity>> ExportAsync(TSpecification specification,
        CancellationToken ct)
    {
        var query = GetInitialQueryForList();

        // Apply specification filters and sorting, but skip pagination
        // Specification receives QueryConfiguration and decides whether to use it or override
        // InvalidSortFieldException will propagate to global handler if sort field is invalid
        query = specification.ApplyWithoutPagination(query, QueryConfiguration);

        // Execute the query and return all matching results
        return await query.ToListAsync(ct);
    }
}