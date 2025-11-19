using MyTodos.BuildingBlocks.Application.Contracts.Persistence;
using TaskEntity = MyTodos.Services.TodoService.Domain.TaskAggregate.Task;

namespace MyTodos.Services.TodoService.Application.Tasks.Contracts;

public interface ITaskReadRepository : IReadRepository<TaskEntity, Guid>
{
    System.Threading.Tasks.Task<TaskEntity?> GetByIdWithDetailsAsync(Guid taskId, CancellationToken ct = default);
}
