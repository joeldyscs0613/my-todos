using MyTodos.BuildingBlocks.Application.Contracts.Persistence;
using MyTodos.Services.TodoService.Domain.ProjectAggregate;

namespace MyTodos.Services.TodoService.Application.Projects.Contracts;

public interface IProjectWriteRepository : IWriteRepository<Project, Guid>
{
}
