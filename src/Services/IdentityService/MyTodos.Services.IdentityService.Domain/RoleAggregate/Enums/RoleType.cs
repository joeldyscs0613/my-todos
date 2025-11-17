namespace MyTodos.Services.IdentityService.Domain.RoleAggregate.Enums;

/// <summary>
/// Defines the type of role for type-safe role identification.
/// </summary>
public enum RoleType
{
    /// <summary>
    /// Global administrator with system-wide access across all tenants
    /// </summary>
    GlobalAdmin = 1,

    /// <summary>
    /// Administrator within a specific tenant
    /// </summary>
    TenantAdmin = 2,

    /// <summary>
    /// Standard user within a specific tenant
    /// </summary>
    TenantUser = 3
}
