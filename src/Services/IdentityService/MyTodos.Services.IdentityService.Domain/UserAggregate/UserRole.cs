using MyTodos.Services.IdentityService.Domain.RoleAggregate;
using MyTodos.Services.IdentityService.Domain.TenantAggregate;
using MyTodos.SharedKernel.Abstractions;

namespace MyTodos.Services.IdentityService.Domain.UserAggregate;

/// <summary>
/// Represents the assignment of a role to a user, optionally scoped to a specific tenant.
/// This is part of the User aggregate and should only be created through User methods.
/// </summary>
public sealed class UserRole : Entity<Guid>
{
    /// <summary>
    /// ID of the user
    /// </summary>
    public Guid UserId { get; private set; }

    /// <summary>
    /// ID of the role
    /// </summary>
    public Guid RoleId { get; private set; }

    /// <summary>
    /// Optional tenant ID (null for global roles, set for tenant-specific roles)
    /// </summary>
    public Guid? TenantId { get; private set; }

    // Navigation properties for queries (not used in domain logic)
    public User User { get; private set; } = null!;
    public Role Role { get; private set; } = null!;
    public Tenant? Tenant { get; private set; }

    // Private parameterless constructor for EF Core
    [Obsolete("Only for deserialization. Use Create method.", error: true)]
    [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
    private UserRole() : base() { }

    // Private constructor for factory methods
    private UserRole(Guid id) : base(id) { }

    /// <summary>
    /// Create a global role assignment (no tenant restriction).
    /// Internal - only accessible within the User aggregate.
    /// </summary>
    /// <param name="userId">The user ID</param>
    /// <param name="roleId">The role ID</param>
    internal static UserRole CreateGlobalRole(Guid userId, Guid roleId)
    {
        if (userId == Guid.Empty)
            throw new DomainException("UserId cannot be empty");

        if (roleId == Guid.Empty)
            throw new DomainException("RoleId cannot be empty");

        var userRoleId = Guid.NewGuid();
        var userRole = new UserRole(userRoleId)
        {
            UserId = userId,
            RoleId = roleId,
            TenantId = null
        };

        return userRole;
    }

    /// <summary>
    /// Create a tenant-scoped role assignment.
    /// Internal - only accessible within the User aggregate.
    /// </summary>
    /// <param name="userId">The user ID</param>
    /// <param name="roleId">The role ID</param>
    /// <param name="tenantId">The tenant ID</param>
    internal static UserRole CreateTenantRole(Guid userId, Guid roleId, Guid tenantId)
    {
        if (userId == Guid.Empty)
            throw new DomainException("UserId cannot be empty");

        if (roleId == Guid.Empty)
            throw new DomainException("RoleId cannot be empty");

        if (tenantId == Guid.Empty)
            throw new DomainException("TenantId cannot be empty");

        var userRoleId = Guid.NewGuid();
        var userRole = new UserRole(userRoleId)
        {
            UserId = userId,
            RoleId = roleId,
            TenantId = tenantId
        };

        return userRole;
    }
}
