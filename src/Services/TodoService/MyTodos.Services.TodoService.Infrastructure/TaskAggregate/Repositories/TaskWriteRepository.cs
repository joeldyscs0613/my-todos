using MyTodos.BuildingBlocks.Application.Contracts.Security;
using MyTodos.BuildingBlocks.Infrastructure.Persistence.Abstractions.Repositories;
using MyTodos.Services.TodoService.Application.Tasks;
using MyTodos.Services.TodoService.Application.Tasks.Contracts;
using MyTodos.Services.TodoService.Application.Tasks.Queries;
using MyTodos.Services.TodoService.Infrastructure.Persistence;
using TaskEntity = MyTodos.Services.TodoService.Domain.TaskAggregate.Task;

namespace MyTodos.Services.TodoService.Infrastructure.TaskAggregate.Repositories;

public sealed class TaskWriteRepository
    : WriteEfRepository<TaskEntity, Guid, TodoServiceDbContext>
    , ITaskWriteRepository
{
    public TaskWriteRepository(TodoServiceDbContext context, ICurrentUserService currentUserService)
        : base(context, new TaskQueryConfiguration(), currentUserService)
    {
    }
}
