using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace MyTodos.BuildingBlocks.Presentation.Configuration;

/// <summary>
/// CORS configuration for API access from frontend applications.
/// </summary>
public static class CorsConfiguration
{
    private const string DefaultPolicyName = "DefaultCorsPolicy";
    private const string AllowedOriginsKey = "Cors:AllowedOrigins";

    public static IServiceCollection AddCorsConfiguration(
        this IServiceCollection services,
        IConfiguration configuration,
        IHostEnvironment environment)
    {
        services.AddCors(options =>
        {
            options.AddPolicy(DefaultPolicyName, builder =>
            {
                if (environment.IsDevelopment())
                {
                    // Development: Allow any origin for local testing
                    builder.AllowAnyOrigin()
                           .AllowAnyMethod()
                           .AllowAnyHeader();
                }
                else
                {
                    // Production: Use configured allowed origins
                    var allowedOrigins = configuration.GetSection(AllowedOriginsKey).Get<string[]>()
                        ?? Array.Empty<string>();

                    if (allowedOrigins.Length > 0)
                    {
                        builder.WithOrigins(allowedOrigins)
                               .AllowAnyMethod()
                               .AllowAnyHeader()
                               .AllowCredentials();
                    }
                    else
                    {
                        // Fallback: No origins allowed if not configured
                        builder.WithOrigins() // Empty = no origins allowed
                               .AllowAnyMethod()
                               .AllowAnyHeader();
                    }
                }
            });
        });

        return services;
    }

    public static IApplicationBuilder UseCorsConfiguration(this IApplicationBuilder app)
    {
        app.UseCors(DefaultPolicyName);
        return app;
    }
}
