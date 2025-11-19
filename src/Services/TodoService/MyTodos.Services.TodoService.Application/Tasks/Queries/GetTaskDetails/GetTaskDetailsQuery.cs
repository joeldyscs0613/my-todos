using FluentValidation;
using MyTodos.BuildingBlocks.Application.Abstractions.Queries;
using MyTodos.BuildingBlocks.Application.Contracts.Security;
using MyTodos.Services.TodoService.Application.Tasks.Contracts;
using MyTodos.Services.TodoService.Domain.TaskAggregate.Enums;
using MyTodos.SharedKernel.Helpers;
using TaskStatus = MyTodos.Services.TodoService.Domain.TaskAggregate.Enums.TaskStatus;

namespace MyTodos.Services.TodoService.Application.Tasks.Queries.GetTaskDetails;

public sealed class GetTaskDetailsQuery(Guid taskId) : Query<TaskDetailsResponseDto>
{
    public Guid TaskId { get; init; } = taskId;
}

public sealed class GetTaskDetailsQueryValidator : AbstractValidator<GetTaskDetailsQuery>
{
    public GetTaskDetailsQueryValidator()
    {
        RuleFor(x => x.TaskId).NotEmpty();
    }
}

public sealed class GetTaskDetailsQueryHandler : QueryHandler<GetTaskDetailsQuery, TaskDetailsResponseDto>
{
    private readonly ITaskReadRepository _taskReadRepository;
    private readonly ICurrentUserService _currentUserService;

    public GetTaskDetailsQueryHandler(
        ITaskReadRepository taskReadRepository,
        ICurrentUserService currentUserService)
    {
        _taskReadRepository = taskReadRepository;
        _currentUserService = currentUserService;
    }

    public override async Task<Result<TaskDetailsResponseDto>> Handle(GetTaskDetailsQuery request, CancellationToken ct)
    {
        var task = await _taskReadRepository.GetByIdWithDetailsAsync(request.TaskId, ct);

        if (task == null)
        {
            return NotFound($"Task with ID '{request.TaskId}' not found");
        }

        // Enforce tenant isolation
        if (!_currentUserService.TenantId.HasValue)
        {
            return Unauthorized("Tenant ID is not available");
        }

        if (task.TenantId != _currentUserService.TenantId.Value)
        {
            return Forbidden("Access denied");
        }

        var dto = new TaskDetailsResponseDto(
            task.Id,
            task.TenantId,
            Guid.Empty, // TODO: CreatedByUserId - need to add this property to Task domain model or store in audit table
            task.AssigneeUserId,
            task.ProjectId,
            task.Title,
            task.Description,
            task.Status,
            task.Priority,
            task.StartDate,
            task.TargetDate,
            task.CompletedAt,
            task.Tags.Select(t => t.Name).ToList(),
            task.Comments.Select(c => new TaskCommentResponseDto(
                c.Id,
                c.Text,
                Guid.Empty, // TODO: CreatedByUserId - need to add this property to TaskComment domain model or store in audit table
                c.CreatedDate)).ToList(),
            task.Attachments.Select(a => new TaskAttachmentResponseDto(
                a.Id,
                a.FileName,
                a.UrlOrStorageKey,
                a.ContentType,
                a.CreatedDate)).ToList(),
            task.CreatedDate,
            task.ModifiedDate ?? task.CreatedDate);

        return Success(dto);
    }
}

public sealed record TaskDetailsResponseDto(
    Guid Id,
    Guid TenantId,
    Guid CreatedByUserId,
    Guid? AssigneeUserId,
    Guid? ProjectId,
    string Title,
    string? Description,
    TaskStatus Status,
    Priority Priority,
    DateTimeOffset? StartDate,
    DateTimeOffset? TargetDate,
    DateTimeOffset? CompletedAtUtc,
    List<string> Tags,
    List<TaskCommentResponseDto> Comments,
    List<TaskAttachmentResponseDto> Attachments,
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset LastUpdatedAtUtc);

public sealed record TaskCommentResponseDto(
    Guid Id,
    string Text,
    Guid CreatedByUserId,
    DateTimeOffset CreatedAtUtc);

public sealed record TaskAttachmentResponseDto(
    Guid Id,
    string FileName,
    string UrlOrStorageKey,
    string ContentType,
    DateTimeOffset CreatedAtUtc);
