using FluentValidation;
using MyTodos.BuildingBlocks.Application.Abstractions.Commands;
using MyTodos.BuildingBlocks.Application.Contracts.Persistence;
using MyTodos.BuildingBlocks.Application.Contracts.Security;
using MyTodos.Services.TodoService.Application.Tasks.Contracts;
using MyTodos.Services.TodoService.Domain.TaskAggregate.Constants;
using MyTodos.Services.TodoService.Domain.TaskAggregate.Enums;
using MyTodos.SharedKernel;
using MyTodos.SharedKernel.Helpers;

namespace MyTodos.Services.TodoService.Application.Tasks.Commands.UpdateTaskGeneralInfo;

public sealed class UpdateTaskGeneralInfoCommand : Command
{
    public Guid TaskId { get; init; }
    public string Title { get; init; } = string.Empty;
    public string? Description { get; init; }
    public Priority Priority { get; init; }
    public DateTime? StartDate { get; init; }
    public DateTime? TargetDate { get; init; }
}

public sealed class UpdateTaskGeneralInfoCommandValidator : AbstractValidator<UpdateTaskGeneralInfoCommand>
{
    public UpdateTaskGeneralInfoCommandValidator()
    {
        RuleFor(x => x.TaskId).NotEmpty();

        RuleFor(x => x.Title)
            .NotEmpty()
            .MaximumLength(TaskConstants.FieldLengths.TitleMaxLength);

        RuleFor(x => x.Description)
            .MaximumLength(TaskConstants.FieldLengths.DescriptionMaxLength)
            .When(x => x.Description != null);
    }
}

public sealed class UpdateTaskGeneralInfoCommandHandler : CommandHandler<UpdateTaskGeneralInfoCommand>
{
    private readonly ITaskWriteRepository _taskWriteRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateTaskGeneralInfoCommandHandler(
        ITaskWriteRepository taskWriteRepository,
        ICurrentUserService currentUserService,
        IUnitOfWork unitOfWork)
    {
        _taskWriteRepository = taskWriteRepository;
        _currentUserService = currentUserService;
        _unitOfWork = unitOfWork;
    }

    public override async Task<Result> Handle(UpdateTaskGeneralInfoCommand request, CancellationToken ct)
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

        var updateResult = task.UpdateGeneralInfo(
            request.Title,
            request.Description);

        if (updateResult.IsFailure)
        {
            var error = updateResult.FirstError;
            return Failure(error.Type, error.Description);
        }

        await _taskWriteRepository.UpdateAsync(task, ct);
        await _unitOfWork.CommitAsync(ct);

        return Success();
    }
}
