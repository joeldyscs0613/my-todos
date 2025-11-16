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
    /// FluentValidation, and pipeline behaviors.
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

        // Future: Register shared application services here
        // services.AddScoped<ISharedService, SharedService>();

        return services;
    }
}
