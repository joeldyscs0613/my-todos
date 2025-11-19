using Microsoft.EntityFrameworkCore;
using MyTodos.BuildingBlocks.Application.Contracts.Queries;
using TaskEntity = MyTodos.Services.TodoService.Domain.TaskAggregate.Task;

namespace MyTodos.Services.TodoService.Application.Tasks.Queries;

public sealed class TaskQueryConfiguration : IEntityQueryConfiguration<TaskEntity>
{
    public IQueryable<TaskEntity> ConfigureAggregate(IQueryable<TaskEntity> query)
    {
        return query
            .Include(t => t.Tags)
            .Include(t => t.Comments)
            .Include(t => t.Attachments);
    }
}
