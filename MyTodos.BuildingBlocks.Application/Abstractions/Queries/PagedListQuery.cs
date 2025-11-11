using MyTodos.BuildingBlocks.Application.Abstractions.Filters;
using MyTodos.BuildingBlocks.Application.Abstractions.Specifications;
using MyTodos.BuildingBlocks.Application.Contracts;
using MyTodos.BuildingBlocks.Application.Helpers;
using MyTodos.SharedKernel.Abstractions;
using MyTodos.SharedKernel.Helpers;

namespace MyTodos.BuildingBlocks.Application.Abstractions.Queries;

public abstract class PagedListQuery<TSpecification, TFilter, TResponseItemDto> 
    : Query<PagedList<TResponseItemDto>>
    where TFilter : Filter
    where TSpecification : class
{
    public TFilter Filter { get; protected set; }
    public int PageNumber => Filter.PageNumber;
    public int PageSize => Filter.PageSize;
    public TSpecification Specification { get; protected set; }
    
    protected PagedListQuery(TFilter filter)
    {
        Filter = filter;
        
        var specification = (Activator.CreateInstance(typeof(TSpecification), filter) as TSpecification);
        Specification = specification ?? throw new InvalidOperationException($" {typeof(TSpecification)} not created");
    }

    protected PagedListQuery()
    {
    }
}

public abstract class PagedListQueryHandler
    <TEntity, TId, TSpecification, TFilter, TQuery, TResponseItemDto>
    : QueryHandler<TQuery, PagedList<TResponseItemDto>>
    where TEntity : AggregateRoot<TId>
    where TId : IComparable
    where TQuery : PagedListQuery<TSpecification, TFilter, TResponseItemDto>
    where TSpecification : Specification<TEntity, TId, TFilter>
    where TFilter : Filter
{
    protected readonly IPagedListReadRepository<TEntity, TId, TSpecification, TFilter> ReadRepository;

    protected PagedListQueryHandler(IPagedListReadRepository<TEntity, TId, TSpecification, TFilter> readRepository)
    {
        ReadRepository = readRepository;
    }

    public override async Task<Result<PagedList<TResponseItemDto>>> Handle(TQuery request, CancellationToken ct)
    {
        var error = await ValidateRequest(request, ct);

        if (error != Error.None)
        {
            return Result.Failure<PagedList<TResponseItemDto>>(error);
        }

        var entities = await ReadRepository.GetPagedListAsync(request.Specification, ct);
        var resultList = GetResultList(request, entities);
        var pageList = PagedList<TResponseItemDto>
            .Create(resultList, request.Specification.TotalCount, request.PageNumber, request.PageSize,
                request.Filter.SortField, request.Filter.SortDirection);
        var result = Result.Create(pageList);

        return result;
    }

    protected virtual async Task<Error> ValidateRequest(TQuery request, CancellationToken ct)
    {
        await Task.CompletedTask;

        return Error.None;
    }

    protected abstract List<TResponseItemDto> GetResultList(TQuery request, IReadOnlyList<TEntity> list);
}