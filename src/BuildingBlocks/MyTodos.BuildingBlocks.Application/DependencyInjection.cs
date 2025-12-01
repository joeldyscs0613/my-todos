using System.Reflection;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using MyTodos.BuildingBlocks.Application.Behaviors;

namespace MyTodos.BuildingBlocks.Application;

/// <summary>
/// Dependency injection setup for BuildingBlocks application layer.
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Registers BuildingBlocks application services including MediatR,
    /// FluentValidation, pipeline behaviors, and domain event handlers.
    /// </summary>
    /// <param name="services">Service collection</param>
    /// <param name="serviceAssembly">The service-specific application assembly (for handler/validator discovery)</param>
    public static IServiceCollection AddBuildingBlocksApplication(
        this IServiceCollection services,
        Assembly serviceAssembly)
    {
        // Register MediatR with handlers from the service assembly
        services.AddMediatR(config =>
        {
            config.RegisterServicesFromAssembly(serviceAssembly);

            // Add BuildingBlocks pipeline behaviors (in execution order)
            config.AddOpenBehavior(typeof(ValidationBehavior<,>));
            config.AddOpenBehavior(typeof(UnitOfWorkBehavior<,>));
            config.AddOpenBehavior(typeof(LoggingBehavior<,>));
        });

        // Register FluentValidation validators from the service assembly
        services.AddValidatorsFromAssembly(serviceAssembly);

        // Auto-register domain event handlers from the service assembly
        services.RegisterDomainEventHandlers(serviceAssembly);

        // Future: Register shared application services here
        // services.AddScoped<ISharedService, SharedService>();

        return services;
    }

    /// <summary>
    /// Automatically discovers and registers all IDomainEventHandler implementations from the assembly.
    /// </summary>
    private static IServiceCollection RegisterDomainEventHandlers(
        this IServiceCollection services,
        Assembly assembly)
    {
        var domainEventHandlerType = typeof(Contracts.DomainEvents.IDomainEventHandler<>);

        var handlerTypes = assembly.GetTypes()
            .Where(t => t.IsClass && !t.IsAbstract)
            .SelectMany(t => t.GetInterfaces()
                .Where(i => i.IsGenericType &&
                           i.GetGenericTypeDefinition() == domainEventHandlerType)
                .Select(i => new { Implementation = t, Interface = i }))
            .ToList();

        foreach (var handler in handlerTypes)
        {
            services.AddScoped(handler.Interface, handler.Implementation);
        }

        return services;
    }
}
