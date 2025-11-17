using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;

namespace MyTodos.BuildingBlocks.Presentation.Configuration;

/// <summary>
/// Swagger/OpenAPI configuration for API documentation with JWT authentication support.
/// </summary>
public static class SwaggerConfiguration
{
    public static IServiceCollection AddSwaggerConfiguration(
        this IServiceCollection services,
        string apiTitle,
        string apiVersion = "v1",
        string? apiDescription = null)
    {
        services.AddEndpointsApiExplorer();

        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc(apiVersion, new OpenApiInfo
            {
                Title = apiTitle,
                Version = apiVersion,
                Description = apiDescription ?? $"{apiTitle} - Built with Clean Architecture + DDD + CQRS",
                Contact = new OpenApiContact
                {
                    Name = "API Support",
                    Email = "support@mytodos.com"
                }
            });

            // Add JWT Bearer authentication to Swagger
            options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Name = "Authorization",
                Type = SecuritySchemeType.Http,
                Scheme = "Bearer",
                BearerFormat = "JWT",
                In = ParameterLocation.Header,
                Description = "JWT Authorization header using the Bearer scheme. Enter your token in the text input below."
            });

            options.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "Bearer"
                        }
                    },
                    Array.Empty<string>()
                }
            });

            // Enable XML documentation if available
            var xmlFiles = Directory.GetFiles(AppContext.BaseDirectory, "*.xml");
            foreach (var xmlFile in xmlFiles)
            {
                options.IncludeXmlComments(xmlFile, includeControllerXmlComments: true);
            }

            // Custom operation filter for permission documentation
            options.OperationFilter<PermissionOperationFilter>();
        });

        return services;
    }
}
