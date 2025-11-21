using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MyTodos.BuildingBlocks.Presentation.Authorization;
using MyTodos.BuildingBlocks.Presentation.Configuration;
using MyTodos.BuildingBlocks.Presentation.Middleware;

namespace MyTodos.BuildingBlocks.Presentation;

/// <summary>
/// Dependency injection configuration for common API/Presentation layer concerns.
/// Includes exception handling, authorization, Swagger, CORS, and API conventions.
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Adds all BuildingBlocks presentation services including exception handling,
    /// permission authorization, Swagger, and CORS.
    /// </summary>
    public static IServiceCollection AddBuildingBlocksPresentation(
        this IServiceCollection services,
        IConfiguration configuration,
        IHostEnvironment environment,
        string apiTitle,
        string apiVersion = "v1",
        string? apiDescription = null)
    {
        // Add controllers with API conventions and JSON configuration
        services.AddControllers()
            .AddJsonOptions(options =>
            {
                // Configure JSON serialization to use string names for enums instead of numeric values
                options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
            });

        // Add HTTP context accessor for authorization handlers
        services.AddHttpContextAccessor();

        // Add global exception handling
        services.AddExceptionHandler<MyTodos.BuildingBlocks.Presentation.Middleware.GlobalExceptionHandler>();
        services.AddSingleton<MyTodos.BuildingBlocks.Presentation.ProblemDetails.ProblemDetailsFactory>();

        // Add permission-based authorization
        services.AddPermissionAuthorization();

        // Add Swagger/OpenAPI documentation
        services.AddSwaggerConfiguration(apiTitle, apiVersion, apiDescription);

        // Add CORS configuration
        services.AddCorsConfiguration(configuration, environment);

        // Configure Problem Details
        services.AddProblemDetails();

        return services;
    }

    /// <summary>
    /// Configures the HTTP request pipeline with BuildingBlocks middleware.
    /// Must be called after authentication/authorization middleware.
    /// </summary>
    public static IApplicationBuilder UseBuildingBlocksPresentation(
        this IApplicationBuilder app,
        IHostEnvironment environment)
    {
        // Global exception handler (must be first)
        app.UseExceptionHandler(options => { });

        // Swagger UI (development only)
        if (environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI(options =>
            {
                options.SwaggerEndpoint("/swagger/v1/swagger.json", "API v1");
                options.RoutePrefix = "swagger";
                options.DisplayRequestDuration();
                options.EnableTryItOutByDefault();
            });
        }

        // CORS (before auth)
        app.UseCorsConfiguration();

        return app;
    }
}
