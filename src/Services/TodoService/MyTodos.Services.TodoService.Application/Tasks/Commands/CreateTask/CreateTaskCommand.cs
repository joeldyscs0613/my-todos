using FluentValidation;
using MyTodos.BuildingBlocks.Application.Abstractions.Commands;
using MyTodos.BuildingBlocks.Application.Abstractions.Dtos;
using MyTodos.BuildingBlocks.Application.Contracts.Persistence;
using MyTodos.BuildingBlocks.Application.Contracts.Security;
using MyTodos.Services.TodoService.Application.Tasks.Contracts;
using MyTodos.Services.TodoService.Domain.TaskAggregate.Constants;
using MyTodos.Services.TodoService.Domain.TaskAggregate.Enums;
using MyTodos.SharedKernel.Helpers;
using TaskEntity = MyTodos.Services.TodoService.Domain.TaskAggregate.Task;

namespace MyTodos.Services.TodoService.Application.Tasks.Commands.CreateTask;

public sealed class CreateTaskCommand : CreateCommand<Guid>
{
    public string Title { get; init; } = string.Empty;
    public string? Description { get; init; }
    public Priority Priority { get; init; } = Priority.NoPriority;
    public DateTime? StartDate { get; init; }
    public DateTime? TargetDate { get; init; }
    public Guid? ProjectId { get; init; }
    public Guid? AssigneeUserId { get; init; }
}

public sealed class CreateTaskCommandValidator : AbstractValidator<CreateTaskCommand>
{
    public CreateTaskCommandValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty()
            .WithMessage(TaskConstants.ErrorMessages.TitleRequired)
            .MaximumLength(TaskConstants.FieldLengths.TitleMaxLength)
            .WithMessage(string.Format(TaskConstants.ErrorMessages.TitleTooLong, TaskConstants.FieldLengths.TitleMaxLength));

        RuleFor(x => x.Description)
            .MaximumLength(TaskConstants.FieldLengths.DescriptionMaxLength)
            .WithMessage(string.Format(TaskConstants.ErrorMessages.DescriptionTooLong, TaskConstants.FieldLengths.DescriptionMaxLength))
            .When(x => x.Description != null);
    }
}

public sealed class CreateTaskCommandHandler : CreateCommandHandler<CreateTaskCommand, Guid>
{
    private readonly ITaskWriteRepository _taskWriteRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IUnitOfWork _unitOfWork;

    public CreateTaskCommandHandler(
        ITaskWriteRepository taskWriteRepository,
        ICurrentUserService currentUserService,
        IUnitOfWork unitOfWork)
    {
        _taskWriteRepository = taskWriteRepository;
        _currentUserService = currentUserService;
        _unitOfWork = unitOfWork;
    }

    public override async Task<Result<CreateCommandResponseDto<Guid>>> Handle(
        CreateTaskCommand request,
        CancellationToken ct)
    {
        if (!_currentUserService.TenantId.HasValue)
        {
            return Unauthorized("Tenant ID is not available");
        }

        if (!_currentUserService.UserId.HasValue)
        {
            return Unauthorized("User ID is not available");
        }

        var userId = _currentUserService.UserId.Value;

        var taskResult = TaskEntity.Create(
            userId,
            request.Title,
            request.Description,
            request.Priority,
            request.StartDate,
            request.TargetDate);

        if (taskResult.IsFailure)
        {
            return Failure(taskResult.FirstError);
        }

        var task = taskResult.Value!;

        // Link to project if provided (LinkToProject returns Result for business rule validation)
        if (request.ProjectId.HasValue)
        {
            // TODO: Get projectTenantId from project repository to validate it belongs to the same tenant
            var linkResult = task.LinkToProject(request.ProjectId.Value, _currentUserService.TenantId.Value);
            if (linkResult.IsFailure)
            {
                return Failure(linkResult.FirstError);
            }
        }

        // Assign if provided (AssignTo returns Result for business rule validation)
        if (request.AssigneeUserId.HasValue)
        {
            // TODO: Get assigneeTenantId from user repository to validate assignee belongs to the same tenant
            var assignResult = task.AssignTo(request.AssigneeUserId.Value, _currentUserService.TenantId.Value);
            if (assignResult.IsFailure)
            {
                return Failure(assignResult.FirstError);
            }
        }

        await _taskWriteRepository.AddAsync(task, ct);
        await _unitOfWork.CommitAsync(ct);

        return Success(task.Id);
    }
}
