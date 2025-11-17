using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;

namespace MyTodos.BuildingBlocks.Presentation.Authorization;

/// <summary>
/// Dynamic authorization policy provider that creates permission policies on-demand.
/// This allows [HasPermission] attribute to work without pre-registering all possible permission combinations.
/// </summary>
public sealed class PermissionPolicyProvider : IAuthorizationPolicyProvider
{
    private readonly DefaultAuthorizationPolicyProvider _fallbackPolicyProvider;
    private const string PermissionPolicyPrefix = "Permission:";

    public PermissionPolicyProvider(IOptions<AuthorizationOptions> options)
    {
        _fallbackPolicyProvider = new DefaultAuthorizationPolicyProvider(options);
    }

    public Task<AuthorizationPolicy> GetDefaultPolicyAsync()
    {
        return _fallbackPolicyProvider.GetDefaultPolicyAsync();
    }

    public Task<AuthorizationPolicy?> GetFallbackPolicyAsync()
    {
        return _fallbackPolicyProvider.GetFallbackPolicyAsync();
    }

    public Task<AuthorizationPolicy?> GetPolicyAsync(string policyName)
    {
        // Check if this is a permission policy
        if (policyName.StartsWith(PermissionPolicyPrefix, StringComparison.OrdinalIgnoreCase))
        {
            // Extract permissions from policy name
            var permissions = policyName.Substring(PermissionPolicyPrefix.Length)
                .Split(',', StringSplitOptions.RemoveEmptyEntries);

            // Build policy with permission requirement
            var policy = new AuthorizationPolicyBuilder()
                .RequireAuthenticatedUser()
                .RequirePermission(permissions)
                .Build();

            return Task.FromResult<AuthorizationPolicy?>(policy);
        }

        // Fall back to default policy provider
        return _fallbackPolicyProvider.GetPolicyAsync(policyName);
    }
}
