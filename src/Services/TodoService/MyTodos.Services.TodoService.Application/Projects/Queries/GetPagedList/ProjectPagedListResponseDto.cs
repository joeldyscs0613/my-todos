using MyTodos.Services.TodoService.Domain.ProjectAggregate.Enums;

namespace MyTodos.Services.TodoService.Application.Projects.Queries.GetPagedList;

public sealed record ProjectPagedListResponseDto(
    Guid Id,
    Guid TenantId,
    string CreatedBy,
    string Name,
    string? Description,
    ProjectStatus Status,
    DateTimeOffset? StartDate,
    DateTimeOffset? TargetDate,
    string? Color,
    string? Icon,
    DateTimeOffset CreatedDate,
    DateTimeOffset? ModifiedDate
);
