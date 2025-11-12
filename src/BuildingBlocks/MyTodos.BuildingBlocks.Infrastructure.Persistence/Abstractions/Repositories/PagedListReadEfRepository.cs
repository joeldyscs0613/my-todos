using Microsoft.EntityFrameworkCore;
using MyTodos.BuildingBlocks.Application.Abstractions.Filters;
using MyTodos.BuildingBlocks.Application.Contracts;
using MyTodos.SharedKernel.Abstractions;

namespace MyTodos.BuildingBlocks.Infrastructure.Persistence.Abstractions.Repositories;

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
    /// <exception cref="InvalidOperationException">Thrown when specification configuration is invalid (e.g., bad sort field).</exception>
    public virtual async Task<IReadOnlyList<TEntity>> GetPagedListAsync(TSpecification specification,
        CancellationToken ct)
    {
        var query = GetInitialQueryForList();

        // Apply specification (includes, filters, search, sorting, pagination)
        var applyResult = specification.Apply(query);

        // Check if specification application failed (e.g., invalid sort field)
        if (applyResult.IsFailure)
        {
            throw new InvalidOperationException(
                $"Failed to apply specification: {applyResult.FirstError.Description}");
        }

        // Execute the query and return the list
        return await applyResult.Value.ToListAsync(ct);
    }
}