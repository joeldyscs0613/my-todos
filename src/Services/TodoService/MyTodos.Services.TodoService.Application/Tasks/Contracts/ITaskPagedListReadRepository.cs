using MyTodos.BuildingBlocks.Application.Contracts.Persistence;
using MyTodos.Services.TodoService.Application.Tasks.Queries.GetPagedList;
using TaskEntity = MyTodos.Services.TodoService.Domain.TaskAggregate.Task;

namespace MyTodos.Services.TodoService.Application.Tasks.Contracts;

/// <summary>
/// Paged list read repository for Task aggregate.
/// </summary>
public interface ITaskPagedListReadRepository
    : IPagedListReadRepository<TaskEntity, Guid, TaskPagedListSpecification, TaskPagedListFilter>
{
    /// <summary>
    /// Get task by ID with all details (tags, comments, attachments)
    /// </summary>
    System.Threading.Tasks.Task<TaskEntity?> GetByIdWithDetailsAsync(Guid taskId, CancellationToken ct = default);
}
