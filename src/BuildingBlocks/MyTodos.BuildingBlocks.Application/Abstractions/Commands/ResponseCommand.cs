using MediatR;
using MyTodos.BuildingBlocks.Application.Contracts;
using MyTodos.BuildingBlocks.Application.Contracts.Commands;
using MyTodos.SharedKernel.Helpers;

namespace MyTodos.BuildingBlocks.Application.Abstractions.Commands;

public abstract class ResponseCommand<TResponseDto> : IRequest<Result<TResponseDto>>, ICommand
{
}

public abstract class ResponseCommandHandler<TCommand, TResponseDto> 
    : IRequestHandler<TCommand, Result<TResponseDto>>
    where TCommand : ResponseCommand<TResponseDto>
{
    protected ResponseCommandHandler() {}
    
    public abstract Task<Result<TResponseDto>> Handle(TCommand request, CancellationToken ct);

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
    
    protected virtual Result<TResponseDto> Conflict(string description)
    {
        return Result.Conflict<TResponseDto>(description);
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