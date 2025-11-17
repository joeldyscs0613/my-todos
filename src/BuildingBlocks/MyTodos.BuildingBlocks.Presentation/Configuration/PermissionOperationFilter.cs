using Microsoft.AspNetCore.Authorization;
using Microsoft.OpenApi.Models;
using MyTodos.BuildingBlocks.Presentation.Authorization;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Reflection;

namespace MyTodos.BuildingBlocks.Presentation.Configuration;

/// <summary>
/// Swagger operation filter that documents permission requirements in API documentation.
/// </summary>
public sealed class PermissionOperationFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        // Get HasPermission attributes from method and controller
        var hasPermissionAttributes = context.MethodInfo
            .GetCustomAttributes<HasPermissionAttribute>()
            .Concat(context.MethodInfo.DeclaringType?.GetCustomAttributes<HasPermissionAttribute>() ?? Enumerable.Empty<HasPermissionAttribute>())
            .ToList();

        if (hasPermissionAttributes.Any())
        {
            // Extract permissions from policy names
            var permissions = hasPermissionAttributes
                .Select(attr => attr.Policy?.Replace("Permission:", ""))
                .Where(p => !string.IsNullOrEmpty(p))
                .SelectMany(p => p!.Split(','))
                .Distinct()
                .ToList();

            if (permissions.Any())
            {
                // Add permission information to operation description
                var permissionsText = string.Join(" OR ", permissions.Select(p => $"`{p}`"));
                var permissionNote = $"\n\n**Required Permissions:** {permissionsText}";

                operation.Description = (operation.Description ?? string.Empty) + permissionNote;

                // Ensure operation has security requirement
                if (!operation.Security.Any())
                {
                    operation.Security.Add(new OpenApiSecurityRequirement
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
                }
            }
        }
        // Check for any Authorize attribute (not HasPermission)
        else if (context.MethodInfo.GetCustomAttributes<AuthorizeAttribute>().Any() ||
                 context.MethodInfo.DeclaringType?.GetCustomAttributes<AuthorizeAttribute>().Any() == true)
        {
            // Ensure operation has security requirement
            if (!operation.Security.Any())
            {
                operation.Security.Add(new OpenApiSecurityRequirement
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
            }
        }
    }
}
