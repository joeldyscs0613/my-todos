using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace MyTodos.Services.TodoService.Infrastructure.Persistence;

/// <summary>
/// Design-time DbContext factory for EF Core migrations.
/// </summary>
public sealed class TodoServiceDbContextFactory : IDesignTimeDbContextFactory<TodoServiceDbContext>
{
    public TodoServiceDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<TodoServiceDbContext>();
        optionsBuilder.UseSqlite("Data Source=todoservice.db");

        return new TodoServiceDbContext(optionsBuilder.Options);
    }
}
