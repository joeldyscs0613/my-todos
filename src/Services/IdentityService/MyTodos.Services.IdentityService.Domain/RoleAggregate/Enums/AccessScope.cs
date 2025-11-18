namespace MyTodos.Services.IdentityService.Domain.RoleAggregate.Enums;

/// <summary>
/// Defines the scope at which access (roles/permissions) applies.
/// </summary>
public enum AccessScope
{
    /// <summary>
    /// System-wide access across all tenants (e.g., GlobalAdmin role)
    /// </summary>
    Global = 10,

    /// <summary>
    /// Tenant-scoped access within specific tenant context (e.g., TenantAdmin, TenantUser roles)
    /// </summary>
    Tenant = 20
}
