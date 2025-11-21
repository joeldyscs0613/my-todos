using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;

namespace MyTodos.BuildingBlocks.Presentation.Authorization;

/// <summary>
/// Extension methods for registering permission-based authorization.
/// </summary>
public static class PermissionAuthorizationExtensions
{
    /// <summary>
    /// Adds permission-based authorization with dynamic policy creation.
    /// </summary>
    public static IServiceCollection AddPermissionAuthorization(this IServiceCollection services)
    {
        services.AddSingleton<IAuthorizationHandler, PermissionAuthorizationHandler>();
        services.AddSingleton<IAuthorizationHandler, ConditionalAuthenticationHandler>();

        // Add authorization with fallback policy that allows anonymous paths like /health
        services.AddAuthorization(options =>
        {
            options.FallbackPolicy = new AuthorizationPolicyBuilder()
                .AddAuthenticationSchemes("Bearer")
                .AddRequirements(new ConditionalAuthenticationRequirement())
                .Build();
            options.InvokeHandlersAfterFailure = false;
        });

        // Register IAuthorizationPolicyProvider for dynamic policy creation
        services.AddSingleton<IAuthorizationPolicyProvider, PermissionPolicyProvider>();

        return services;
    }

    /// <summary>
    /// Extension method to build a permission requirement.
    /// </summary>
    public static AuthorizationPolicyBuilder RequirePermission(
        this AuthorizationPolicyBuilder builder,
        params string[] permissions)
    {
        builder.AddRequirements(new PermissionRequirement(permissions));
        return builder;
    }
}
