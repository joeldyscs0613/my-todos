using MyTodos.Services.TodoService.Domain.TaskAggregate.Enums;

namespace MyTodos.Services.TodoService.Application.Tasks.Queries.GetPagedList;

public sealed record TaskPagedListResponseDto(
    Guid Id,
    Guid TenantId,
    Guid? ProjectId,
    Guid CreatedByUserId,
    Guid? AssignedToUserId,
    string Title,
    string? Description,
    Domain.TaskAggregate.Enums.TaskStatus Status,
    Priority Priority,
    DateTimeOffset? StartDate,
    DateTimeOffset? TargetDate,
    bool IsRecurring,
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset LastUpdatedAtUtc
);
