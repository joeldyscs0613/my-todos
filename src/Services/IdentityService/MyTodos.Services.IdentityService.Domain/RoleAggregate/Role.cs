using MyTodos.Services.IdentityService.Domain.RoleAggregate.Enums;
using MyTodos.Services.IdentityService.Domain.UserAggregate;
using MyTodos.SharedKernel;
using MyTodos.SharedKernel.Abstractions;
using MyTodos.SharedKernel.Helpers;

namespace MyTodos.Services.IdentityService.Domain.RoleAggregate;

/// <summary>
/// Represents a role in the system (e.g., GlobalAdmin, TenantAdmin, TenantUser).
/// Roles are shared across all tenants.
/// </summary>
public sealed class Role : AggregateRoot<Guid>
{
    /// <summary>
    /// Role type for type-safe identification
    /// </summary>
    public RoleType RoleType { get; private set; }

    /// <summary>
    /// Unique code for this role (e.g., "global-admin", "tenant-admin")
    /// </summary>
    public string Code { get; private set; } = string.Empty;

    /// <summary>
    /// Display name for the role (e.g., "Global Administrator")
    /// </summary>
    public string Name { get; private set; } = string.Empty;

    /// <summary>
    /// Optional description of the role
    /// </summary>
    public string? Description { get; private set; }

    /// <summary>
    /// Scope at which this role can be assigned (Global or Tenant)
    /// </summary>
    public AccessScope Scope { get; private set; }

    // Child entities within Role aggregate
    private readonly List<RolePermission> _rolePermissions = new();
    public IReadOnlyCollection<RolePermission> RolePermissions => _rolePermissions;

    // Navigation property for UserRole queries (read-only in domain)
    public ICollection<UserRole> UserRoles { get; private set; } = new List<UserRole>();

    // Private parameterless constructor for EF Core
    [Obsolete("Only for deserialization. Use Create method.", error: true)]
    [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
    private Role() : base() { }

    // Constructor for factory method
    private Role(Guid id) : base(id) { }

    /// <summary>
    /// Create a new role
    /// </summary>
    /// <param name="roleType">The role type</param>
    /// <param name="code">Unique role code</param>
    /// <param name="name">Display name</param>
    /// <param name="scope">Access scope (Global or Tenant)</param>
    /// <param name="description">Optional description</param>
    public static Result<Role> Create(
        RoleType roleType,
        string code,
        string name,
        AccessScope scope,
        string? description = null)
    {
        if (string.IsNullOrWhiteSpace(code))
            return Result.Failure<Role>(Error.BadRequest("Role code cannot be empty"));

        if (string.IsNullOrWhiteSpace(name))
            return Result.Failure<Role>(Error.BadRequest("Role name cannot be empty"));

        var roleId = Guid.NewGuid();
        var role = new Role(roleId)
        {
            RoleType = roleType,
            Code = code,
            Name = name,
            Scope = scope,
            Description = description
        };

        return Result.Success(role);
    }

    /// <summary>
    /// Update role details
    /// </summary>
    public Result Update(string name, string? description = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            return Result.Failure(Error.BadRequest("Role name cannot be empty"));

        Name = name;
        Description = description;
        return Result.Success();
    }

    /// <summary>
    /// Assign a permission to this role
    /// </summary>
    /// <param name="permissionId">The ID of the permission to assign</param>
    public void AssignPermission(Guid permissionId)
    {
        if (_rolePermissions.Any(rp => rp.PermissionId == permissionId))
            return; // Already has this permission

        var rolePermission = RolePermission.Create(Id, permissionId);
        _rolePermissions.Add(rolePermission);
    }

    /// <summary>
    /// Remove a permission from this role
    /// </summary>
    /// <param name="permissionId">The ID of the permission to remove</param>
    public void RemovePermission(Guid permissionId)
    {
        var rolePermission = _rolePermissions.FirstOrDefault(rp => rp.PermissionId == permissionId);
        if (rolePermission != null)
        {
            _rolePermissions.Remove(rolePermission);
        }
    }

    /// <summary>
    /// Check if this role has a specific permission
    /// </summary>
    /// <param name="permissionId">The permission ID to check</param>
    public bool HasPermission(Guid permissionId)
    {
        return _rolePermissions.Any(rp => rp.PermissionId == permissionId);
    }

    /// <summary>
    /// Get all permission IDs assigned to this role
    /// </summary>
    public IEnumerable<Guid> GetPermissionIds()
    {
        return _rolePermissions.Select(rp => rp.PermissionId);
    }
}
