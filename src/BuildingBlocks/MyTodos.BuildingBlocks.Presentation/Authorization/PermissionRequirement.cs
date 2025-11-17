using Microsoft.AspNetCore.Authorization;

namespace MyTodos.BuildingBlocks.Presentation.Authorization;

/// <summary>
/// Authorization requirement for permission-based access control.
/// Supports multiple permissions (any match grants access) and wildcard matching.
/// </summary>
public sealed class PermissionRequirement : IAuthorizationRequirement
{
    public string[] Permissions { get; }

    public PermissionRequirement(params string[] permissions)
    {
        if (permissions == null || permissions.Length == 0)
        {
            throw new ArgumentException("At least one permission is required", nameof(permissions));
        }

        Permissions = permissions;
    }
}
