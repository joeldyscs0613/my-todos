using Microsoft.Extensions.DependencyInjection;
using MyTodos.BuildingBlocks.Application;

namespace MyTodos.Services.TodoService.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddTodoServiceApplication(this IServiceCollection services)
    {
        var assembly = typeof(DependencyInjection).Assembly;

        // BuildingBlocks handles MediatR, validators, and behaviors
        services.AddBuildingBlocksApplication(assembly);

        // Future: Register TodoService-specific application services here

        return services;
    }
}
