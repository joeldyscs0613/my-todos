using MyTodos.BuildingBlocks.Application.Contracts.Persistence;
using MyTodos.Services.TodoService.Domain.ProjectAggregate;

namespace MyTodos.Services.TodoService.Application.Projects.Contracts;

public interface IProjectReadRepository : IReadRepository<Project, Guid>
{
    Task<Project?> GetByIdWithDetailsAsync(Guid projectId, CancellationToken ct = default);
}
