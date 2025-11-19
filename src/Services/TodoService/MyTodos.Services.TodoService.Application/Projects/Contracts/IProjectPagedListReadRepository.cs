using MyTodos.BuildingBlocks.Application.Contracts.Persistence;
using MyTodos.Services.TodoService.Application.Projects.Queries.GetPagedList;
using MyTodos.Services.TodoService.Domain.ProjectAggregate;

namespace MyTodos.Services.TodoService.Application.Projects.Contracts;

/// <summary>
/// Paged list read repository for Project aggregate.
/// </summary>
public interface IProjectPagedListReadRepository
    : IPagedListReadRepository<Project, Guid, ProjectPagedListSpecification, ProjectPagedListFilter>
{
    /// <summary>
    /// Get project by ID with all details
    /// </summary>
    Task<Project?> GetByIdWithDetailsAsync(Guid projectId, CancellationToken ct = default);
}
