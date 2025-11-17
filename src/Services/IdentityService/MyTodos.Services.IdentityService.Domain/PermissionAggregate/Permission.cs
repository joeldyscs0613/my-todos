using MyTodos.Services.IdentityService.Domain.RoleAggregate;
using MyTodos.SharedKernel.Abstractions;

namespace MyTodos.Services.IdentityService.Domain.PermissionAggregate;

/// <summary>
/// Represents a permission (capability) in the system.
/// Permissions are global entities that can be assigned to roles.
/// Supports wildcards: "*" for all permissions, "namespace.*" for all permissions in a namespace.
/// </summary>
public sealed class Permission : AggregateRoot<Guid>
{
    /// <summary>
    /// Unique code for this permission (e.g., "users.create", "todos.*", "*")
    /// </summary>
    public string Code { get; private set; } = string.Empty;

    /// <summary>
    /// Display name for this permission
    /// </summary>
    public string Name { get; private set; } = string.Empty;

    /// <summary>
    /// Optional description of what this permission grants
    /// </summary>
    public string? Description { get; private set; }

    // Navigation property for queries (read-only in domain)
    public ICollection<RolePermission> RolePermissions { get; private set; } = new List<RolePermission>();

    // Private parameterless constructor for EF Core
    [Obsolete("Only for deserialization. Use Create method.", error: true)]
    [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
    private Permission() : base() { }

    // Constructor for factory method
    private Permission(Guid id) : base(id) { }

    /// <summary>
    /// Create a new permission
    /// </summary>
    /// <param name="code">Unique permission code (e.g., "users.create", "todos.*", "*")</param>
    /// <param name="name">Display name</param>
    /// <param name="description">Optional description</param>
    public static Permission Create(string code, string name, string? description = null)
    {
        if (string.IsNullOrWhiteSpace(code))
            throw new DomainException("Permission code cannot be empty");

        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("Permission name cannot be empty");

        var permissionId = Guid.NewGuid();
        var permission = new Permission(permissionId)
        {
            Code = code,
            Name = name,
            Description = description
        };

        return permission;
    }

    /// <summary>
    /// Update permission details
    /// </summary>
    public void Update(string name, string? description = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("Permission name cannot be empty");

        Name = name;
        Description = description;
    }

    /// <summary>
    /// Check if this permission matches the requested permission code.
    /// Supports wildcard matching.
    /// </summary>
    /// <param name="requestedPermission">The permission code to check (e.g., "users.create")</param>
    /// <returns>True if this permission grants access to the requested permission</returns>
    public bool Matches(string requestedPermission)
    {
        if (string.IsNullOrWhiteSpace(requestedPermission))
            return false;

        // Exact match
        if (Code == requestedPermission)
            return true;

        // Global wildcard
        if (Code == "*")
            return true;

        // Namespace wildcard (e.g., "users.*" matches "users.create")
        if (Code.EndsWith(".*"))
        {
            var codeNamespace = Code[..^2]; // Remove ".*"
            var requestedNamespace = requestedPermission.Split('.')[0];
            return codeNamespace == requestedNamespace;
        }

        return false;
    }
}
