using MyTodos.BuildingBlocks.Application.Contracts.Security;
using MyTodos.BuildingBlocks.Infrastructure.Persistence.Abstractions.Repositories;
using MyTodos.Services.TodoService.Application.Tasks.Contracts;
using MyTodos.Services.TodoService.Application.Tasks.Queries;
using TaskEntity = MyTodos.Services.TodoService.Domain.TaskAggregate.Task;

namespace MyTodos.Services.TodoService.Infrastructure.Persistence.Tasks.Repositories;

public sealed class TaskReadRepository
    : ReadEfRepository<TaskEntity, Guid, TodoServiceDbContext>
    , ITaskReadRepository
{
    public TaskReadRepository(TodoServiceDbContext context, ICurrentUserService currentUserService)
        : base(context, new TaskQueryConfiguration(), currentUserService)
    {
    }

    public async System.Threading.Tasks.Task<TaskEntity?> GetByIdWithDetailsAsync(Guid taskId, CancellationToken ct = default)
    {
        return await GetByIdAsync(taskId, ct);
    }
}
