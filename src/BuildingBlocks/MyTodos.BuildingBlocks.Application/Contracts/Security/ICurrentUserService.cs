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
}