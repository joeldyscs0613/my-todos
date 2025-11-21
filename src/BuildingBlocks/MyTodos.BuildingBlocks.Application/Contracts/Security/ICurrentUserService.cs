using System.Security.Claims;

namespace MyTodos.BuildingBlocks.Application.Contracts.Security;

/// <summary>
/// Provides access to the current user's identity and context.
/// </summary>
public interface ICurrentUserService
{
    /// <summary>
    /// Unique identifier for the current user
    /// </summary>
    Guid? UserId { get; }

    /// <summary>
    /// Username of the current user
    /// </summary>
    string? Username { get; }

    /// <summary>
    /// Tenant ID the current user belongs to
    /// </summary>
    Guid? TenantId { get; }

    /// <summary>
    /// Indicates if the current request is authenticated
    /// </summary>
    bool IsAuthenticated { get; }

    /// <summary>
    /// Gets the list of roles assigned to the current user
    /// </summary>
    /// <returns>List of role names/codes</returns>
    List<string> GetRoles();

    /// <summary>
    /// Gets the list of permissions granted to the current user
    /// </summary>
    /// <returns>List of permission codes</returns>
    List<string> GetPermissions();

    /// <summary>
    /// Checks if the current user is a Global Administrator.
    /// Global Administrators have full access to all tenants and platform management.
    /// </summary>
    /// <returns>True if the user is a Global Administrator, false otherwise</returns>
    bool IsGlobalAdmin();

    /// <summary>
    /// Checks if the current user is a Tenant Administrator.
    /// Tenant Administrators can manage users within their own tenant.
    /// </summary>
    /// <returns>True if the user is a Tenant Administrator, false otherwise</returns>
    bool IsTenantAdmin();
}