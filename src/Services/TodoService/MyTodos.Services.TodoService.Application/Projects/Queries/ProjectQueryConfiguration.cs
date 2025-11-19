using MyTodos.BuildingBlocks.Application.Contracts.Queries;
using MyTodos.Services.TodoService.Domain.ProjectAggregate;

namespace MyTodos.Services.TodoService.Application.Projects.Queries;

public sealed class ProjectQueryConfiguration : IEntityQueryConfiguration<Project>
{
    public IQueryable<Project> ConfigureAggregate(IQueryable<Project> query)
    {
        return query;
    }
}
