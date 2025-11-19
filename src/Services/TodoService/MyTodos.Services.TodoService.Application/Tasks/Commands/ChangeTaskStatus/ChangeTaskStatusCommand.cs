using FluentValidation;
using MyTodos.BuildingBlocks.Application.Abstractions.Commands;
using MyTodos.BuildingBlocks.Application.Contracts.Persistence;
using MyTodos.BuildingBlocks.Application.Contracts.Security;
using MyTodos.Services.TodoService.Application.Tasks.Contracts;
using MyTodos.SharedKernel.Helpers;
using TaskStatus = MyTodos.Services.TodoService.Domain.TaskAggregate.Enums.TaskStatus;

namespace MyTodos.Services.TodoService.Application.Tasks.Commands.ChangeTaskStatus;

public sealed class ChangeTaskStatusCommand : Command
{
    public Guid TaskId { get; init; }
    public TaskStatus Status { get; init; }
}

public sealed class ChangeTaskStatusCommandValidator : AbstractValidator<ChangeTaskStatusCommand>
{
    public ChangeTaskStatusCommandValidator()
    {
        RuleFor(x => x.TaskId).NotEmpty();
        RuleFor(x => x.Status).IsInEnum();
    }
}

public sealed class ChangeTaskStatusCommandHandler : CommandHandler<ChangeTaskStatusCommand>
{
    private readonly ITaskWriteRepository _taskWriteRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IUnitOfWork _unitOfWork;

    public ChangeTaskStatusCommandHandler(
        ITaskWriteRepository taskWriteRepository,
        ICurrentUserService currentUserService,
        IUnitOfWork unitOfWork)
    {
        _taskWriteRepository = taskWriteRepository;
        _currentUserService = currentUserService;
        _unitOfWork = unitOfWork;
    }

    public override async Task<Result> Handle(ChangeTaskStatusCommand request, CancellationToken ct)
    {
        var task = await _taskWriteRepository.GetByIdAsync(request.TaskId, ct);
        if (task == null)
        {
            return NotFound($"Task with ID '{request.TaskId}' not found");
        }

        if (!_currentUserService.TenantId.HasValue)
        {
            return Unauthorized("Tenant ID is not available");
        }

        if (task.TenantId != _currentUserService.TenantId.Value)
        {
            return Forbidden("Access denied to this resource");
        }

        // Domain method is now void - structural validation handled by FluentValidation
        task.ChangeStatus(request.Status);

        await _taskWriteRepository.UpdateAsync(task, ct);
        await _unitOfWork.CommitAsync(ct);

        return Success();
    }
}
