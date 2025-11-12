using MyTodos.BuildingBlocks.Application.Abstractions.Filters;
using MyTodos.SharedKernel.Abstractions;

namespace MyTodos.BuildingBlocks.Application.Contracts;

public interface IPagedListReadRepository<TEntity, in TId, in TSpecification, in TFilter> 
    : IReadRepository<TEntity, TId>
    where TEntity : AggregateRoot<TId>
    where TId : IComparable
    where TSpecification : class, ISpecification<TEntity, TFilter>
    where TFilter : Filter
{
    Task<IReadOnlyList<TEntity>> GetPagedListAsync(TSpecification specification, CancellationToken ct);
}