using System.Security.Claims;
using MyTodos.BuildingBlocks.Application.Constants;

namespace MyTodos.Services.IdentityService.Contracts;

/// <summary>
/// Helper methods for user role and permission checks.
/// Can be used in any service (IdentityService, TodoService, etc.) for consistent role detection.
/// </summary>
public static class UserRoleHelper
{
    /// <summary>
    /// Check if the user is a Global Administrator
    /// </summary>
    public static bool IsGlobalAdmin(ClaimsPrincipal user)
    {
        if (user == null) return false;

        var roles = GetRoles(user);
        return roles.Contains(WellKnownRoles.GlobalAdmin);
    }

    /// <summary>
    /// Check if the user is a Tenant Administrator
    /// </summary>
    public static bool IsTenantAdmin(ClaimsPrincipal user)
    {
        if (user == null) return false;

        var roles = GetRoles(user);
        return roles.Contains(WellKnownRoles.TenantAdmin);
    }

    /// <summary>
    /// Check if the user is an Application Administrator
    /// </summary>
    public static bool IsAppAdmin(ClaimsPrincipal user)
    {
        if (user == null) return false;

        var roles = GetRoles(user);
        return roles.Contains(WellKnownRoles.AppAdmin);
    }

    /// <summary>
    /// Check if user has any admin role (Global, Tenant, or App)
    /// </summary>
    public static bool IsAnyAdmin(ClaimsPrincipal user)
    {
        if (user == null) return false;

        var roles = GetRoles(user);
        return roles.Contains(WellKnownRoles.GlobalAdmin) ||
               roles.Contains(WellKnownRoles.TenantAdmin) ||
               roles.Contains(WellKnownRoles.AppAdmin);
    }

    /// <summary>
    /// Check if user has wildcard permission (super admin)
    /// </summary>
    public static bool HasWildcardPermission(ClaimsPrincipal user)
    {
        if (user == null) return false;

        var permissions = GetPermissions(user);
        return permissions.Contains(WellKnownPermissions.All);
    }

    /// <summary>
    /// Check if user has a specific permission
    /// </summary>
    public static bool HasPermission(ClaimsPrincipal user, string permission)
    {
        if (user == null) return false;

        // Check for wildcard first
        if (HasWildcardPermission(user)) return true;

        var permissions = GetPermissions(user);
        return permissions.Contains(permission);
    }

    /// <summary>
    /// Get all roles for the current user
    /// </summary>
    public static List<string> GetRoles(ClaimsPrincipal user)
    {
        if (user == null) return new List<string>();

        return user.FindAll(ClaimTypes.Role)
            .Select(c => c.Value)
            .ToList();
    }

    /// <summary>
    /// Get all permissions for the current user
    /// </summary>
    public static List<string> GetPermissions(ClaimsPrincipal user)
    {
        if (user == null) return new List<string>();

        return user.FindAll("permission")
            .Select(c => c.Value)
            .ToList();
    }

    /// <summary>
    /// Get the user's tenant ID from claims
    /// </summary>
    public static Guid? GetTenantId(ClaimsPrincipal user)
    {
        if (user == null) return null;

        var tenantIdClaim = user.FindFirst("tenant_id");
        if (tenantIdClaim == null || string.IsNullOrEmpty(tenantIdClaim.Value))
            return null;

        if (Guid.TryParse(tenantIdClaim.Value, out var tenantId))
            return tenantId;

        return null;
    }

    /// <summary>
    /// Get the user's ID from claims
    /// </summary>
    public static Guid? GetUserId(ClaimsPrincipal user)
    {
        if (user == null) return null;

        var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim == null || string.IsNullOrEmpty(userIdClaim.Value))
            return null;

        if (Guid.TryParse(userIdClaim.Value, out var userId))
            return userId;

        return null;
    }
}
