using MyTodos.BuildingBlocks.Application.Abstractions.Filters;
using MyTodos.BuildingBlocks.Application.Contracts.Specifications;
using MyTodos.SharedKernel.Abstractions;

namespace MyTodos.BuildingBlocks.Application.Contracts.Persistence;

/// <summary>
/// Read repository with paginated query support using the Specification pattern.
/// Specifications encapsulate filtering, searching, sorting, and pagination logic.
/// </summary>
/// <typeparam name="TEntity">The aggregate root entity type.</typeparam>
/// <typeparam name="TId">The type of the entity's unique identifier.</typeparam>
/// <typeparam name="TSpecification">The specification type for applying query criteria.</typeparam>
/// <typeparam name="TFilter">The filter type containing pagination, sorting, and search parameters.</typeparam>
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
    Task<IReadOnlyList<TEntity>> GetPagedListAsync(TSpecification specification, CancellationToken ct);

    /// <summary>
    /// Exports all entities matching the specification's filter and sort criteria without pagination.
    /// Used for export scenarios to retrieve all filtered/sorted results.
    /// </summary>
    /// <param name="specification">Specification containing filter criteria and sort order (pagination ignored).</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Read-only list of all entities matching the filter and sort criteria.</returns>
    Task<IReadOnlyList<TEntity>> ExportAsync(TSpecification specification, CancellationToken ct);
}