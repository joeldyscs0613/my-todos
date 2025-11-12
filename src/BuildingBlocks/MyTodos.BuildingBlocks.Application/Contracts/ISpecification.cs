using MyTodos.BuildingBlocks.Application.Abstractions.Filters;
using MyTodos.SharedKernel.Helpers;

namespace MyTodos.BuildingBlocks.Application.Contracts;

public interface ISpecification<TEntity, out TFilter>
    where TFilter : Filter
{
    TFilter Filter { get; }
    int PageNumber { get; }
    public int PageSize { get; }
    int TotalCount { get; }

    public Result<IQueryable<TEntity>> Apply(IQueryable<TEntity> query);
}