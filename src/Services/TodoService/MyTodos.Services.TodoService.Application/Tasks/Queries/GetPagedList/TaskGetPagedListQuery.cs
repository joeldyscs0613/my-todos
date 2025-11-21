using MyTodos.BuildingBlocks.Application.Abstractions.Queries;
using MyTodos.BuildingBlocks.Application.Contracts.Security;
using MyTodos.Services.TodoService.Application.Tasks.Contracts;
using MyTodos.SharedKernel.Helpers;
using TaskEntity = MyTodos.Services.TodoService.Domain.TaskAggregate.Task;

namespace MyTodos.Services.TodoService.Application.Tasks.Queries.GetPagedList;

public sealed class TaskPagedListQuery
    : PagedListQuery<TaskPagedListSpecification, TaskPagedListFilter, TaskPagedListResponseDto>
{
    public TaskPagedListQuery(TaskPagedListFilter filter) : base(filter)
    {
    }
}

public sealed class TaskPagedListQueryHandler(
    ITaskPagedListReadRepository readRepository)
    : PagedListQueryHandler<TaskEntity, Guid, TaskPagedListSpecification, TaskPagedListFilter,
        TaskPagedListQuery, TaskPagedListResponseDto>(readRepository)
{
    protected override List<TaskPagedListResponseDto> GetResultList(
        TaskPagedListQuery request, IReadOnlyList<TaskEntity> list)
    {
        return list.Select(t =>
            new TaskPagedListResponseDto(
                t.Id,
                t.TenantId,
                t.ProjectId,
                Guid.Empty, // TODO: CreatedByUserId - need to add this property to Task domain model or store in audit table
                t.AssigneeUserId,
                t.Title,
                t.Description,
                t.Status,
                t.Priority,
                t.StartDate,
                t.TargetDate,
                false, // TODO: IsRecurring - need to add this property to Task domain model if recurring tasks are needed
                t.CreatedDate,
                t.ModifiedDate ?? t.CreatedDate))
            .ToList();
    }
}
