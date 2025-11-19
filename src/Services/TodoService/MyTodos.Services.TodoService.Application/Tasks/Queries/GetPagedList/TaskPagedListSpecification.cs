using System.Linq.Expressions;
using MyTodos.BuildingBlocks.Application.Abstractions.Specifications;
using TaskEntity = MyTodos.Services.TodoService.Domain.TaskAggregate.Task;

namespace MyTodos.Services.TodoService.Application.Tasks.Queries.GetPagedList;

public sealed class TaskPagedListSpecification(TaskPagedListFilter filter)
    : Specification<TaskEntity, Guid, TaskPagedListFilter>(filter)
{
    protected override IQueryable<TaskEntity> ApplyFilter(IQueryable<TaskEntity> query)
    {
        if (Filter.TaskId.HasValue && Filter.TaskId != Guid.Empty)
        {
            query = query.Where(t => t.Id == Filter.TaskId);
        }

        if (Filter.ProjectId.HasValue && Filter.ProjectId != Guid.Empty)
        {
            query = query.Where(t => t.ProjectId == Filter.ProjectId);
        }

        if (Filter.TenantId.HasValue && Filter.TenantId != Guid.Empty)
        {
            query = query.Where(t => t.TenantId == Filter.TenantId);
        }

        if (Filter.AssignedToUserId.HasValue && Filter.AssignedToUserId != Guid.Empty)
        {
            query = query.Where(t => t.AssigneeUserId == Filter.AssignedToUserId);
        }

        // TODO: CreatedByUserId filtering - need to add this property to Task domain model or query from audit table
        // if (Filter.CreatedByUserId.HasValue && Filter.CreatedByUserId != Guid.Empty)
        // {
        //     query = query.Where(t => t.CreatedByUserId == Filter.CreatedByUserId);
        // }

        if (Filter.Status.HasValue)
        {
            query = query.Where(t => t.Status == Filter.Status);
        }

        if (Filter.Priority.HasValue)
        {
            query = query.Where(t => t.Priority == Filter.Priority);
        }

        // TODO: IsRecurring filtering - need to add this property to Task domain model if recurring tasks are needed
        // if (Filter.IsRecurring.HasValue)
        // {
        //     query = query.Where(t => t.IsRecurring == Filter.IsRecurring);
        // }

        if (Filter.DueAfter.HasValue)
        {
            query = query.Where(t => t.TargetDate >= Filter.DueAfter);
        }

        if (Filter.DueBefore.HasValue)
        {
            query = query.Where(t => t.TargetDate <= Filter.DueBefore);
        }

        return query;
    }

    protected override IQueryable<TaskEntity> ApplySearchBy(IQueryable<TaskEntity> query)
    {
        if (!string.IsNullOrWhiteSpace(Filter.SearchBy))
        {
            query = query.Where(t => t.Title.Contains(Filter.SearchBy)
                             || (t.Description != null && t.Description.Contains(Filter.SearchBy)));
        }

        return query;
    }

    protected override Dictionary<string, Expression<Func<TaskEntity, object>>> GetSortFunctions()
    {
        return new Dictionary<string, Expression<Func<TaskEntity, object>>>
        {
            { "Title", t => t.Title },
            { "Status", t => t.Status },
            { "Priority", t => t.Priority },
            { "TargetDate", t => t.TargetDate ?? DateTimeOffset.MaxValue },
            { "StartDate", t => t.StartDate ?? DateTimeOffset.MaxValue },
            { "CreatedAtUtc", t => t.CreatedDate },
            { "LastUpdatedAtUtc", t => t.ModifiedDate ?? t.CreatedDate }
        };
    }
}
