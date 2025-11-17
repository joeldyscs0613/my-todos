using Microsoft.AspNetCore.Authorization;

namespace MyTodos.BuildingBlocks.Presentation.Authorization;

/// <summary>
/// Authorization attribute for permission-based access control.
/// Usage: [HasPermission("users.create")] or [HasPermission("users.create", "users.update")]
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
public sealed class HasPermissionAttribute : AuthorizeAttribute
{
    public HasPermissionAttribute(params string[] permissions)
    {
        if (permissions == null || permissions.Length == 0)
        {
            throw new ArgumentException("At least one permission is required", nameof(permissions));
        }

        // Create a unique policy name based on the permissions
        // This allows the same permission combination to reuse the same policy
        Policy = $"Permission:{string.Join(",", permissions.OrderBy(p => p))}";
    }
}
