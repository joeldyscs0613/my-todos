using MyTodos.BuildingBlocks.Application.Contracts.Security;
using MyTodos.BuildingBlocks.Infrastructure.Persistence.Abstractions.Repositories;
using MyTodos.Services.TodoService.Application.Projects.Contracts;
using MyTodos.Services.TodoService.Application.Projects.Queries;
using MyTodos.Services.TodoService.Application.Projects.Queries.GetPagedList;
using MyTodos.Services.TodoService.Domain.ProjectAggregate;

namespace MyTodos.Services.TodoService.Infrastructure.Persistence.Projects.Repositories;

/// <summary>
/// Paged list read repository for Project aggregate queries.
/// </summary>
public sealed class ProjectPagedListReadRepository(TodoServiceDbContext context, ICurrentUserService currentUserService)
    : PagedListReadEfRepository<Project, Guid, ProjectPagedListSpecification, ProjectPagedListFilter, TodoServiceDbContext>(
        context, new ProjectQueryConfiguration(), currentUserService)
    , IProjectPagedListReadRepository
{
    public async Task<Project?> GetByIdWithDetailsAsync(Guid projectId, CancellationToken ct = default)
        => await GetFirstOrDefaultAsync(p => p.Id == projectId, ct);
}
