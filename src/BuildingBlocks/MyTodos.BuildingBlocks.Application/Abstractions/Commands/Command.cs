using MediatR;
using MyTodos.BuildingBlocks.Application.Contracts;
using MyTodos.BuildingBlocks.Application.Contracts.Commands;
using MyTodos.SharedKernel.Helpers;

namespace MyTodos.BuildingBlocks.Application.Abstractions.Commands;

public abstract class Command : IRequest<Result>, ICommand
{
}

public abstract class CommandHandler<TCommand> : IRequestHandler<TCommand, Result>
    where TCommand : Command
{
    protected CommandHandler()
    {
    }

    public abstract Task<Result> Handle(TCommand request, CancellationToken ct);
    
    protected virtual Result Success()
    {
        return Result.Success();
    }
    
    protected virtual Result BadRequest(string description)
    {
        return Result.BadRequest(description);
    }
    
    protected virtual Result NotFound(string description)
    {
        return Result.NotFound(description);
    }
    
    protected virtual Result Conflict(string description)
    {
        return Result.Conflict(description);
    }
    
    protected virtual Result Forbidden(string description)
    {
        return Result.Forbidden(description);
    }
    
    protected virtual Result Unauthorized(string description)
    {
        return Result.Unauthorized(description);
    }
    
    protected virtual Result Failure(ErrorType errorType, string description)
    {
        return Result.Failure(new Error(errorType, description));
    }
}