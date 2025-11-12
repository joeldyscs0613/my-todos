using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using MyTodos.BuildingBlocks.Presentation.Middleware;
using MyTodos.BuildingBlocks.Presentation.ProblemDetails;

namespace MyTodos.BuildingBlocks.Presentation.Extensions;

/// <summary>
/// Extension methods for registering presentation layer middleware and services.
/// </summary>
public static class MiddlewareExtensions
{
    /// <summary>
    /// Registers the global exception handler and related services in the dependency injection container.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddGlobalExceptionHandler(this IServiceCollection services)
    {
        services.AddTransient<GlobalExceptionHandler>();
        services.AddSingleton<ProblemDetailsFactory>();

        return services;
    }

    /// <summary>
    /// Adds the global exception handler middleware to the application pipeline.
    /// This should be one of the first middleware components to catch all unhandled exceptions.
    /// </summary>
    /// <param name="app">The application builder.</param>
    /// <returns>The application builder for chaining.</returns>
    public static IApplicationBuilder UseGlobalExceptionHandler(this IApplicationBuilder app)
    {
        app.UseMiddleware<GlobalExceptionHandler>();

        return app;
    }
}
