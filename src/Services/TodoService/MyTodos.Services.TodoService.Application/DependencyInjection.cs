using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using MyTodos.BuildingBlocks.Application.Behaviors;

namespace MyTodos.Services.TodoService.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddTodoServiceApplication(this IServiceCollection services)
    {
        var assembly = typeof(DependencyInjection).Assembly;

        // Register MediatR with all handlers from this assembly
        services.AddMediatR(config =>
        {
            config.RegisterServicesFromAssembly(assembly);

            // Add pipeline behaviors (order matters - executed in registration order)
            config.AddOpenBehavior(typeof(ValidationBehavior<,>));
            config.AddOpenBehavior(typeof(UnitOfWorkBehavior<,>));
            config.AddOpenBehavior(typeof(LoggingBehavior<,>));
        });

        // Register all FluentValidation validators from this assembly
        services.AddValidatorsFromAssembly(assembly);

        return services;
    }
}
