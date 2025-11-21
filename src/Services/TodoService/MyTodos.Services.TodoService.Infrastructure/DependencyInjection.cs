using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MyTodos.BuildingBlocks.Application.Contracts.Persistence;
using MyTodos.BuildingBlocks.Application.Contracts.Security;
using MyTodos.BuildingBlocks.Infrastructure;
using MyTodos.Services.TodoService.Application.Projects.Contracts;
using MyTodos.Services.TodoService.Application.Shared.Contracts;
using MyTodos.Services.TodoService.Application.Tasks.Contracts;
using MyTodos.Services.TodoService.Infrastructure.Persistence;
using MyTodos.Services.TodoService.Infrastructure.Persistence.Projects.Repositories;
using MyTodos.Services.TodoService.Infrastructure.Persistence.Shared;
using MyTodos.Services.TodoService.Infrastructure.Persistence.Tasks.Repositories;
using MyTodos.Services.TodoService.Infrastructure.Security;

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

        // 2. CurrentUserService - Register extended version BEFORE calling AddBuildingBlocksInfrastructure
        // This allows us to use TodoServiceCurrentUserService instead of the base CurrentUserService
        services.AddScoped<ICurrentUserService, TodoServiceCurrentUserService>();

        // 3. UnitOfWork
        services.AddScoped<IUnitOfWork, TodoServiceUnitOfWork>();

        // 4. Repositories (depend on ICurrentUserService)
        services.AddScoped<ITaskReadRepository, TaskReadRepository>();
        services.AddScoped<ITaskWriteRepository, TaskWriteRepository>();
        services.AddScoped<ITaskPagedListReadRepository, TaskPagedListReadRepository>();
        services.AddScoped<IProjectReadRepository, ProjectReadRepository>();
        services.AddScoped<IProjectWriteRepository, ProjectWriteRepository>();
        services.AddScoped<IProjectPagedListReadRepository, ProjectPagedListReadRepository>();
        services.AddScoped<IAssigneeOptionsRepository, AssigneeOptionsRepository>();

        // 5. DbContext for BuildingBlocks (important!)
        services.AddScoped<DbContext>(sp => sp.GetRequiredService<TodoServiceDbContext>());

        // 6. Seeding
        services.AddScoped<DatabaseSeederService>();

        // 7. BuildingBlocks infrastructure
        services.AddBuildingBlocksInfrastructure(configuration);

        return services;
    }
}
