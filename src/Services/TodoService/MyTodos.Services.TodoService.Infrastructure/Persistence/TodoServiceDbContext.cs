using Microsoft.EntityFrameworkCore;
using MyTodos.BuildingBlocks.Infrastructure.Persistence.Abstractions;

namespace MyTodos.Services.TodoService.Infrastructure.Persistence;

public sealed class TodoServiceDbContext : BaseDbContext
{
    public TodoServiceDbContext(DbContextOptions options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Apply all configurations from this assembly
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(TodoServiceDbContext).Assembly);
    }
}
