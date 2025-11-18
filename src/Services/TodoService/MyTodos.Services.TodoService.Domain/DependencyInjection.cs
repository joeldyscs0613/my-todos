using Microsoft.Extensions.DependencyInjection;

namespace MyTodos.Services.TodoService.Domain;

public static class DependencyInjection
{
    public static IServiceCollection AddTodoServiceDomain(this IServiceCollection services)
    {
        // Register domain services here if needed
        return services;
    }
}
