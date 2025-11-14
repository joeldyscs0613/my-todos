using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace MyTodos.Services.TodoService.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddTodoServiceInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        return services;
    }
}
