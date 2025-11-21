using MyTodos.BuildingBlocks.Application.Abstractions.Queries;
using MyTodos.BuildingBlocks.Application.Contracts.Security;
using MyTodos.Services.TodoService.Application.Projects.Contracts;
using MyTodos.Services.TodoService.Domain.ProjectAggregate;
using MyTodos.SharedKernel.Helpers;

namespace MyTodos.Services.TodoService.Application.Projects.Queries.GetPagedList;

public sealed class ProjectGetPagedListQuery
    : GetPagedListQuery<ProjectPagedListSpecification, ProjectPagedListFilter, ProjectPagedListResponseDto>
{
    public ProjectGetPagedListQuery(ProjectPagedListFilter filter) : base(filter)
    {
    }
}

public sealed class ProjectGetPagedListQueryHandler(
    IProjectPagedListReadRepository readRepository)
    : GetPagedListQueryHandler<Project, Guid, ProjectPagedListSpecification, ProjectPagedListFilter,
        ProjectGetPagedListQuery, ProjectPagedListResponseDto>(readRepository)
{
    protected override List<ProjectPagedListResponseDto> GetResultList(
        ProjectGetPagedListQuery request, IReadOnlyList<Project> list)
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
