using MediatR;
using MyTodos.SharedKernel.Helpers;

namespace MyTodos.BuildingBlocks.Application.Abstractions.Queries;

public abstract class Query<TResponse> : IRequest<Result<TResponse>>
{
}

public abstract class QueryHandler<TQuery, TResponseDto> 
    : IRequestHandler<TQuery, Result<TResponseDto>>
    where TQuery : Query<TResponseDto>
{
    protected QueryHandler() {}

    public abstract Task<Result<TResponseDto>> Handle(TQuery request, CancellationToken ct);
    
    protected virtual Result<TResponseDto> Success(TResponseDto response)
    {
        return Result.Success(response);
    }
    
    protected virtual Result<TResponseDto> BadRequest(string description)
    {
        return Result.BadRequest<TResponseDto>(description);
    }
    
    protected virtual Result<TResponseDto> NotFound(string description)
    {
        return Result.NotFound<TResponseDto>(description);
    }
    
    protected virtual Result<TResponseDto> Forbidden(string description)
    {
        return Result.Forbidden<TResponseDto>(description);
    }
    
    protected virtual Result<TResponseDto> Unauthorized(string description)
    {
        return Result.Unauthorized<TResponseDto>(description);
    }
    
    protected virtual Result<TResponseDto> Failure(ErrorType errorType, string description)
    {
        return Result.Failure<TResponseDto>(new Error(errorType, description));
    }
}