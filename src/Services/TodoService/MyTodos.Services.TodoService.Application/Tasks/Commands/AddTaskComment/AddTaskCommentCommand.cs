using FluentValidation;
using MyTodos.BuildingBlocks.Application.Abstractions.Commands;
using MyTodos.BuildingBlocks.Application.Contracts.Persistence;
using MyTodos.BuildingBlocks.Application.Contracts.Security;
using MyTodos.Services.TodoService.Application.Tasks.Contracts;
using MyTodos.SharedKernel.Helpers;

namespace MyTodos.Services.TodoService.Application.Tasks.Commands.AddTaskComment;

public sealed class AddTaskCommentCommand : Command
{
    public Guid TaskId { get; init; }
    public string Text { get; init; } = string.Empty;
}

public sealed class AddTaskCommentCommandValidator : AbstractValidator<AddTaskCommentCommand>
{
    public AddTaskCommentCommandValidator()
    {
        RuleFor(x => x.TaskId).NotEmpty();
        RuleFor(x => x.Text).NotEmpty();
    }
}

public sealed class AddTaskCommentCommandHandler : CommandHandler<AddTaskCommentCommand>
{
    private readonly ITaskWriteRepository _taskWriteRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IUnitOfWork _unitOfWork;

    public AddTaskCommentCommandHandler(
        ITaskWriteRepository taskWriteRepository,
        ICurrentUserService currentUserService,
        IUnitOfWork unitOfWork)
    {
        _taskWriteRepository = taskWriteRepository;
        _currentUserService = currentUserService;
        _unitOfWork = unitOfWork;
    }

    public override async Task<Result> Handle(AddTaskCommentCommand request, CancellationToken ct)
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

        if (!_currentUserService.UserId.HasValue)
        {
            return Unauthorized("User ID is not available");
        }

        var userId = _currentUserService.UserId.Value;
        var addCommentResult = task.AddComment(userId, request.Text);

        if (addCommentResult.IsFailure)
        {
            return addCommentResult;
        }

        await _taskWriteRepository.UpdateAsync(task, ct);
        await _unitOfWork.CommitAsync(ct);

        return Success();
    }
}
