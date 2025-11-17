using MyTodos.Services.IdentityService.Domain.PermissionAggregate;
using MyTodos.SharedKernel.Abstractions;

namespace MyTodos.Services.IdentityService.Domain.RoleAggregate;

/// <summary>
/// Represents the many-to-many relationship between Roles and Permissions.
/// This is part of the Role aggregate and should only be created through Role methods.
/// </summary>
public sealed class RolePermission : Entity<Guid>
{
    /// <summary>
    /// ID of the role
    /// </summary>
    public Guid RoleId { get; private set; }

    /// <summary>
    /// ID of the permission
    /// </summary>
    public Guid PermissionId { get; private set; }

    // Navigation properties for queries (not used in domain logic)
    public Role Role { get; private set; } = null!;
    public Permission Permission { get; private set; } = null!;

    // Private parameterless constructor for EF Core
    [Obsolete("Only for deserialization. Use Create method.", error: true)]
    [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
    private RolePermission() : base() { }

    // Private constructor for factory method
    private RolePermission(Guid id) : base(id) { }

    /// <summary>
    /// Create a new role-permission assignment.
    /// Internal - only accessible within the Role aggregate.
    /// </summary>
    /// <param name="roleId">The role ID</param>
    /// <param name="permissionId">The permission ID</param>
    internal static RolePermission Create(Guid roleId, Guid permissionId)
    {
        if (roleId == Guid.Empty)
            throw new DomainException("RoleId cannot be empty");

        if (permissionId == Guid.Empty)
            throw new DomainException("PermissionId cannot be empty");

        var rolePermissionId = Guid.NewGuid();
        var rolePermission = new RolePermission(rolePermissionId)
        {
            RoleId = roleId,
            PermissionId = permissionId
        };

        return rolePermission;
    }
}
