using MyTodos.Services.IdentityService.Domain.TenantAggregate.DomainEvents;
using MyTodos.Services.IdentityService.Domain.UserAggregate;
using MyTodos.SharedKernel;
using MyTodos.SharedKernel.Abstractions;
using MyTodos.SharedKernel.Helpers;

namespace MyTodos.Services.IdentityService.Domain.TenantAggregate;

/// <summary>
/// Represents a tenant (organization/company) in the system.
/// </summary>
public sealed class Tenant : AggregateRoot<Guid>
{
    /// <summary>
    /// Unique name/identifier for the tenant (e.g., company slug)
    /// </summary>
    public string Name { get; private set; } = string.Empty;

    /// <summary>
    /// Whether the tenant is active or suspended
    /// </summary>
    public bool IsActive { get; private set; }

    // Navigation property for queries (read-only in domain)
    public ICollection<UserRole> UserRoles { get; private set; } = new List<UserRole>();

    // Private parameterless constructor for EF Core
    [Obsolete("Only for deserialization. Use Create method.", error: true)]
    [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
    private Tenant() : base() { }

    // Constructor for factory method
    private Tenant(Guid id) : base(id) { }

    /// <summary>
    /// Create a new tenant
    /// </summary>
    public static Result<Tenant> Create(string name, bool isActive)
    {
        if (string.IsNullOrWhiteSpace(name))
            return Result.BadRequest<Tenant>("Tenant name cannot be empty");

        var tenantId = Guid.NewGuid();
        var tenant = new Tenant(tenantId)
        {
            Name = name,
            IsActive = isActive
        };

        tenant.AddDomainEvent(new TenantCreatedDomainEvent(tenant.Id, tenant.Name));

        return Result.Success(tenant);
    }

    /// <summary>
    /// Update tenant general information (name, etc.)
    /// </summary>
    public Result UpdateGeneralInfo(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            return Result.BadRequest("Tenant name cannot be empty");

        Name = name;
        return Result.Success();
    }

    /// <summary>
    /// Deactivate the tenant
    /// </summary>
    public void Deactivate()
    {
        IsActive = false;
    }

    /// <summary>
    /// Reactivate the tenant
    /// </summary>
    public void Activate()
    {
        IsActive = true;
    }

    /// <summary>
    /// Mark tenant for deletion and raise domain event.
    /// This allows business logic to execute before actual deletion (e.g., delete users, clean up resources).
    /// </summary>
    public void Delete()
    {
        // Raise domain event for other parts of the system to react
        // (e.g., delete associated users, notify other services, etc.)
        AddDomainEvent(new TenantDeletedDomainEvent(Id, Name));
    }

    /// <summary>
    /// Get all users belonging to this tenant (via UserRoles navigation)
    /// </summary>
    public IEnumerable<User> GetUsers()
    {
        return UserRoles.Select(ur => ur.User).Distinct();
    }

    /// <summary>
    /// Get the current number of users in this tenant
    /// </summary>
    public int GetUserCount()
    {
        return UserRoles.Select(ur => ur.UserId).Distinct().Count();
    }
}
