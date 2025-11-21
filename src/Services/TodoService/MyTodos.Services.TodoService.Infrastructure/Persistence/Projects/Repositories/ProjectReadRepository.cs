using MyTodos.BuildingBlocks.Application.Contracts.Security;
using MyTodos.BuildingBlocks.Infrastructure.Persistence.Abstractions.Repositories;
using MyTodos.Services.TodoService.Application.Projects.Contracts;
using MyTodos.Services.TodoService.Application.Projects.Queries;
using MyTodos.Services.TodoService.Domain.ProjectAggregate;

namespace MyTodos.Services.TodoService.Infrastructure.Persistence.Projects.Repositories;

public sealed class ProjectReadRepository
    : ReadEfRepository<Project, Guid, TodoServiceDbContext>
    , IProjectReadRepository
{
    public ProjectReadRepository(TodoServiceDbContext context, ICurrentUserService currentUserService)
        : base(context, new ProjectQueryConfiguration(), currentUserService)
    {
    }

    public async Task<Project?> GetByIdWithDetailsAsync(Guid projectId, CancellationToken ct = default)
    {
        return await GetByIdAsync(projectId, ct);
    }
}
