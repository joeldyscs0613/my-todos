using Microsoft.EntityFrameworkCore;
using MyTodos.BuildingBlocks.Application.Contracts.Security;
using MyTodos.BuildingBlocks.Infrastructure.Persistence.Abstractions;
using MyTodos.Services.TodoService.Domain.ProjectAggregate;
using TaskEntity = MyTodos.Services.TodoService.Domain.TaskAggregate.Task;

namespace MyTodos.Services.TodoService.Infrastructure.Persistence;

public sealed class TodoServiceDbContext : BaseDbContext
{
    public TodoServiceDbContext(DbContextOptions options, ICurrentUserService currentUserService)
        : base(options, currentUserService)
    {
    }

    public DbSet<TaskEntity> Tasks => Set<TaskEntity>();
    public DbSet<Project> Projects => Set<Project>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Apply all configurations from this assembly
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(TodoServiceDbContext).Assembly);
    }
}
