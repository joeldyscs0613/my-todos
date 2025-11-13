using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using MyTodos.BuildingBlocks.Presentation.Middleware;
using MyTodos.BuildingBlocks.Presentation.ProblemDetails;

namespace MyTodos.BuildingBlocks.Presentation.Extensions;

/// <summary>
/// Extension methods for registering presentation layer exception handling and services.
/// </summary>
public static class MiddlewareExtensions
{
    /// <summary>
    /// Registers the global exception handler and related services in the dependency injection container.
    /// Uses the modern IExceptionHandler pattern introduced in .NET 7/8.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddGlobalExceptionHandler(this IServiceCollection services)
    {
        services.AddExceptionHandler<GlobalExceptionHandler>();
        services.AddSingleton<ProblemDetailsFactory>();

        return services;
    }

    /// <summary>
    /// Adds the global exception handler to the application pipeline.
    /// This should be one of the first middleware components to catch all unhandled exceptions.
    /// Must be called after AddGlobalExceptionHandler() in service registration.
    /// </summary>
    /// <param name="app">The application builder.</param>
    /// <returns>The application builder for chaining.</returns>
    public static IApplicationBuilder UseGlobalExceptionHandler(this IApplicationBuilder app)
    {
        app.UseExceptionHandler();

        return app;
    }
}
