using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MyTodos.BuildingBlocks.Application.Contracts.Persistence;
using MyTodos.BuildingBlocks.Infrastructure;
using MyTodos.Services.TodoService.Infrastructure.Persistence;
using MyTodos.Services.TodoService.Infrastructure.Seeding;

namespace MyTodos.Services.TodoService.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddTodoServiceInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // 1. DbContext
        var connectionString = configuration.GetConnectionString("TodoServiceDb")
            ?? "Data Source=todoservice.db";

        services.AddDbContext<TodoServiceDbContext>(options =>
            options.UseSqlite(connectionString));

        // 2. UnitOfWork
        services.AddScoped<IUnitOfWork, TodoServiceUnitOfWork>();

        // 3. Repositories
        // Add repository registrations here as they are created
        // Example:
        // services.AddScoped<ITodoItemReadRepository, TodoItemReadRepository>();
        // services.AddScoped<ITodoItemWriteRepository, TodoItemWriteRepository>();

        // 4. DbContext for BuildingBlocks (important!)
        services.AddScoped<DbContext>(sp => sp.GetRequiredService<TodoServiceDbContext>());

        // 5. Seeding
        services.AddScoped<DatabaseSeederService>();

        // 6. BuildingBlocks infrastructure
        services.AddBuildingBlocksInfrastructure(configuration);

        return services;
    }
}
