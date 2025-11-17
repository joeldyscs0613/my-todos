namespace MyTodos.Services.IdentityService.Domain.TenantAggregate.Enums;

/// <summary>
/// Represents the subscription plan tier for a tenant.
/// </summary>
public enum TenantPlan
{
    /// <summary>
    /// Free tier with limited features
    /// </summary>
    Free = 0,

    /// <summary>
    /// Basic tier with standard features
    /// </summary>
    Basic = 1,

    /// <summary>
    /// Premium tier with advanced features
    /// </summary>
    Premium = 2,

    /// <summary>
    /// Enterprise tier with full features and support
    /// </summary>
    Enterprise = 3
}
