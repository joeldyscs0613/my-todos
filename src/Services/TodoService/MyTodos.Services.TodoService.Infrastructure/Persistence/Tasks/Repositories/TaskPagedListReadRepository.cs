using MyTodos.BuildingBlocks.Application.Contracts.Security;
using MyTodos.BuildingBlocks.Infrastructure.Persistence.Abstractions.Repositories;
using MyTodos.Services.TodoService.Application.Tasks.Contracts;
using MyTodos.Services.TodoService.Application.Tasks.Queries;
using MyTodos.Services.TodoService.Application.Tasks.Queries.GetPagedList;
using TaskEntity = MyTodos.Services.TodoService.Domain.TaskAggregate.Task;

namespace MyTodos.Services.TodoService.Infrastructure.Persistence.Tasks.Repositories;

/// <summary>
/// Paged list read repository for Task aggregate queries.
/// </summary>
public sealed class TaskPagedListReadRepository(TodoServiceDbContext context, ICurrentUserService currentUserService)
    : PagedListReadEfRepository<TaskEntity, Guid, TaskPagedListSpecification, TaskPagedListFilter, TodoServiceDbContext>(
        context, new TaskQueryConfiguration(), currentUserService)
    , ITaskPagedListReadRepository
{
    public async System.Threading.Tasks.Task<TaskEntity?> GetByIdWithDetailsAsync(Guid taskId, CancellationToken ct = default)
        => await GetFirstOrDefaultAsync(t => t.Id == taskId, ct);
}
