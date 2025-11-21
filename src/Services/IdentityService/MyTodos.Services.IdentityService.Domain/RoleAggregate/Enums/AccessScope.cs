namespace MyTodos.Services.IdentityService.Domain.RoleAggregate.Enums;

/// <summary>
/// Defines the scope at which access (roles/permissions) applies.
/// </summary>
public enum AccessScope
{
    /// <summary>
    /// Platform-level administration (manages tenants, system config, billing).
    /// Examples: Platform admins who manage the SaaS platform itself.
    /// </summary>
    Global = 10,

    /// <summary>
    /// Organization-level permissions within a tenant boundary.
    /// Examples: Tenant admins who manage users, settings within their organization.
    /// </summary>
    Tenant = 20,

    /// <summary>
    /// Application/workspace-level permissions for regular app usage.
    /// Examples: Users creating projects, tasks, collaborating on work.
    /// </summary>
    App = 30
}
