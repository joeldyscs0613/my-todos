using MyTodos.BuildingBlocks.Application.Contracts.Persistence;
using TaskEntity = MyTodos.Services.TodoService.Domain.TaskAggregate.Task;

namespace MyTodos.Services.TodoService.Application.Tasks.Contracts;

public interface ITaskWriteRepository : IWriteRepository<TaskEntity, Guid>
{
}
