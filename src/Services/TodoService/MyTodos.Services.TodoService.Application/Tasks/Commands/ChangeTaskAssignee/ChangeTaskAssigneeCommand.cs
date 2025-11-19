using FluentValidation;
using MyTodos.BuildingBlocks.Application.Abstractions.Commands;
using MyTodos.BuildingBlocks.Application.Contracts.Persistence;
using MyTodos.BuildingBlocks.Application.Contracts.Security;
using MyTodos.Services.TodoService.Application.Tasks.Contracts;
using MyTodos.SharedKernel.Helpers;

namespace MyTodos.Services.TodoService.Application.Tasks.Commands.ChangeTaskAssignee;

public sealed class ChangeTaskAssigneeCommand : Command
{
    public Guid TaskId { get; init; }
    public Guid? AssigneeUserId { get; init; }
}

public sealed class ChangeTaskAssigneeCommandValidator : AbstractValidator<ChangeTaskAssigneeCommand>
{
    public ChangeTaskAssigneeCommandValidator()
    {
        RuleFor(x => x.TaskId).NotEmpty();
    }
}

public sealed class ChangeTaskAssigneeCommandHandler : CommandHandler<ChangeTaskAssigneeCommand>
{
    private readonly ITaskWriteRepository _taskWriteRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IUnitOfWork _unitOfWork;

    public ChangeTaskAssigneeCommandHandler(
        ITaskWriteRepository taskWriteRepository,
        ICurrentUserService currentUserService,
        IUnitOfWork unitOfWork)
    {
        _taskWriteRepository = taskWriteRepository;
        _currentUserService = currentUserService;
        _unitOfWork = unitOfWork;
    }

    public override async Task<Result> Handle(ChangeTaskAssigneeCommand request, CancellationToken ct)
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

        if (request.AssigneeUserId.HasValue)
        {
            var tenantId = _currentUserService.TenantId.Value;
            var assignResult = task.AssignTo(request.AssigneeUserId.Value, tenantId);
            if (assignResult.IsFailure)
            {
                return assignResult;
            }
        }
        else
        {
            // Domain method is now void - structural validation handled by FluentValidation
            task.Unassign();
        }

        await _taskWriteRepository.UpdateAsync(task, ct);
        await _unitOfWork.CommitAsync(ct);

        return Success();
    }
}
