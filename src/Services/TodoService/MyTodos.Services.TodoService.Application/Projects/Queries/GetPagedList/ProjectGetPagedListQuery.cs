using MyTodos.BuildingBlocks.Application.Abstractions.Queries;
using MyTodos.BuildingBlocks.Application.Contracts.Security;
using MyTodos.Services.TodoService.Application.Projects.Contracts;
using MyTodos.Services.TodoService.Domain.ProjectAggregate;
using MyTodos.SharedKernel.Helpers;

namespace MyTodos.Services.TodoService.Application.Projects.Queries.GetPagedList;

public sealed class ProjectPagedListQuery
    : PagedListQuery<ProjectPagedListSpecification, ProjectPagedListFilter, ProjectPagedListResponseDto>
{
    public ProjectPagedListQuery(ProjectPagedListFilter filter) : base(filter)
    {
    }
}

public sealed class ProjectPagedListQueryHandler(
    IProjectPagedListReadRepository readRepository)
    : PagedListQueryHandler<Project, Guid, ProjectPagedListSpecification, ProjectPagedListFilter,
        ProjectPagedListQuery, ProjectPagedListResponseDto>(readRepository)
{
    protected override List<ProjectPagedListResponseDto> GetResultList(
        ProjectPagedListQuery request, IReadOnlyList<Project> list)
    {
        return list.Select(p =>
            new ProjectPagedListResponseDto(
                p.Id,
                p.TenantId,
                p.CreatedBy,
                p.Name,
                p.Description,
                p.Status,
                p.StartDate,
                p.TargetDate,
                p.Color,
                p.Icon,
                p.CreatedDate.Date,
                p.ModifiedDate?.Date))
            .ToList();
    }
}
