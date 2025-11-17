using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace MyTodos.BuildingBlocks.Presentation.Authorization;

/// <summary>
/// Authorization handler for permission-based access control.
/// Checks if user has required permissions with support for wildcard matching.
/// </summary>
public sealed class PermissionAuthorizationHandler : AuthorizationHandler<PermissionRequirement>
{
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        PermissionRequirement requirement)
    {
        // Get all permission claims from the user
        var userPermissions = context.User
            .FindAll("permission")
            .Select(c => c.Value)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        if (userPermissions.Count == 0)
        {
            return Task.CompletedTask; // No permissions, requirement fails
        }

        // Check if user has super-admin wildcard
        if (userPermissions.Contains("*"))
        {
            context.Succeed(requirement);
            return Task.CompletedTask;
        }

        // Check if user has any of the required permissions (with wildcard support)
        foreach (var requiredPermission in requirement.Permissions)
        {
            if (HasPermission(userPermissions, requiredPermission))
            {
                context.Succeed(requirement);
                return Task.CompletedTask;
            }
        }

        return Task.CompletedTask; // No matching permission, requirement fails
    }

    private static bool HasPermission(HashSet<string> userPermissions, string requiredPermission)
    {
        // Direct match
        if (userPermissions.Contains(requiredPermission))
        {
            return true;
        }

        // Wildcard matching: Check if user has namespace wildcard
        // e.g., "users.*" should grant access to "users.create", "users.delete", etc.
        var parts = requiredPermission.Split('.');
        for (int i = parts.Length - 1; i >= 0; i--)
        {
            var wildcardPermission = string.Join(".", parts.Take(i)) + ".*";
            if (userPermissions.Contains(wildcardPermission))
            {
                return true;
            }
        }

        return false;
    }
}
