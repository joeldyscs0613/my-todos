namespace MyTodos.BuildingBlocks.Application.Contracts.Security;

/// <summary>
/// Extended security service with role and permission management.
/// </summary>
public interface ISecurityService : ICurrentUserService
{
    /// <summary>
    /// Email address of the current user
    /// </summary>
    string? Email { get; }

    /// <summary>
    /// Roles assigned to the current user
    /// </summary>
    IReadOnlyList<string> Roles { get; }

    /// <summary>
    /// Check if user has a specific role
    /// </summary>
    bool IsInRole(string role);

    /// <summary>
    /// Check if user has a specific permission
    /// </summary>
    bool HasPermission(string permission);
}