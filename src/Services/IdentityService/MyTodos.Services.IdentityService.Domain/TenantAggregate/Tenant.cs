using MyTodos.Services.IdentityService.Domain.TenantAggregate.DomainEvents;
using MyTodos.Services.IdentityService.Domain.TenantAggregate.Enums;
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
    /// Subscription plan tier
    /// </summary>
    public TenantPlan Plan { get; private set; }

    /// <summary>
    /// Maximum number of users allowed for this tenant
    /// </summary>
    public int MaxUsers { get; private set; }

    /// <summary>
    /// Whether the tenant is active or suspended
    /// </summary>
    public bool IsActive { get; private set; }

    /// <summary>
    /// When the tenant's subscription expires
    /// </summary>
    public DateTimeOffset? SubscriptionExpiresAt { get; private set; }

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
    public static Result<Tenant> Create(
        string name,
        TenantPlan plan = TenantPlan.Free,
        int maxUsers = 5)
    {
        if (string.IsNullOrWhiteSpace(name))
            return Result.BadRequest<Tenant>("Tenant name cannot be empty");

        var tenantId = Guid.NewGuid();
        var tenant = new Tenant(tenantId)
        {
            Name = name,
            Plan = plan,
            MaxUsers = maxUsers,
            IsActive = true
        };

        tenant.AddDomainEvent(new TenantCreatedDomainEvent(tenant.Id, tenant.Name, tenant.Plan));

        return Result.Success(tenant);
    }

    /// <summary>
    /// Upgrade tenant's subscription plan
    /// </summary>
    public Result UpgradePlan(TenantPlan newPlan, int newMaxUsers, DateTimeOffset? expiresAt = null)
    {
        // Business rule: New plan must be higher tier than current plan
        if (newPlan <= Plan)
        {
            return Result.BadRequest("New plan must be a higher tier than current plan");
        }

        Plan = newPlan;
        MaxUsers = newMaxUsers;
        SubscriptionExpiresAt = expiresAt;

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
