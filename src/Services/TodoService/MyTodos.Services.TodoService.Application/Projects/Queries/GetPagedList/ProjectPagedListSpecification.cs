using System.Linq.Expressions;
using MyTodos.BuildingBlocks.Application.Abstractions.Specifications;
using MyTodos.Services.TodoService.Domain.ProjectAggregate;

namespace MyTodos.Services.TodoService.Application.Projects.Queries.GetPagedList;

public sealed class ProjectPagedListSpecification(ProjectPagedListFilter filter)
    : Specification<Project, Guid, ProjectPagedListFilter>(filter)
{
    protected override IQueryable<Project> ApplyFilter(IQueryable<Project> query)
    {
        if (Filter.ProjectId.HasValue && Filter.ProjectId != Guid.Empty)
        {
            query = query.Where(p => p.Id == Filter.ProjectId);
        }

        if (Filter.TenantId.HasValue && Filter.TenantId != Guid.Empty)
        {
            query = query.Where(p => p.TenantId == Filter.TenantId);
        }

        if (!string.IsNullOrWhiteSpace(Filter.CreatedBy))
        {
            query = query.Where(p => p.CreatedBy == Filter.CreatedBy);
        }

        if (Filter.Status.HasValue)
        {
            query = query.Where(p => p.Status == Filter.Status);
        }

        if (Filter.StartAfter.HasValue)
        {
            query = query.Where(p => p.StartDate >= Filter.StartAfter);
        }

        if (Filter.StartBefore.HasValue)
        {
            query = query.Where(p => p.StartDate <= Filter.StartBefore);
        }

        if (Filter.TargetAfter.HasValue)
        {
            query = query.Where(p => p.TargetDate >= Filter.TargetAfter);
        }

        if (Filter.TargetBefore.HasValue)
        {
            query = query.Where(p => p.TargetDate <= Filter.TargetBefore);
        }

        return query;
    }

    protected override IQueryable<Project> ApplySearchBy(IQueryable<Project> query)
    {
        if (!string.IsNullOrWhiteSpace(Filter.SearchBy))
        {
            query = query.Where(p => p.Name.Contains(Filter.SearchBy)
                             || (p.Description != null && p.Description.Contains(Filter.SearchBy)));
        }

        return query;
    }

    protected override Dictionary<string, Expression<Func<Project, object>>> GetSortFunctions()
    {
        return new Dictionary<string, Expression<Func<Project, object>>>
        {
            { "Name", p => p.Name },
            { "Status", p => p.Status },
            { "StartDate", p => p.StartDate ?? DateTime.MaxValue },
            { "TargetDate", p => p.TargetDate ?? DateTime.MaxValue },
            { "CreatedDate", p => p.CreatedDate },
            { "ModifiedDate", p => p.ModifiedDate }
        };
    }
}
