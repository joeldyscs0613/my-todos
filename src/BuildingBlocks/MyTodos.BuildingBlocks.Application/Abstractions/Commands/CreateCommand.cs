using MyTodos.BuildingBlocks.Application.Abstractions.Dtos;
using MyTodos.SharedKernel.Helpers;

namespace MyTodos.BuildingBlocks.Application.Abstractions.Commands;

public abstract class CreateCommand<TId> : ResponseCommand<CreateCommandResponseDto<TId>>;

public abstract class CreateCommandHandler<TCommand, TId>
    : ResponseCommandHandler<TCommand, CreateCommandResponseDto<TId>>
    where TCommand : CreateCommand<TId>
{
    protected Result<CreateCommandResponseDto<TId>> Success(TId id)
    {
        return Result.Success(new CreateCommandResponseDto<TId>(id));
    }
}