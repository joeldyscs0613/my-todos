namespace MyTodos.BuildingBlocks.Application.Contracts.Security;

/// <summary>
/// Service for generating and validating JWT tokens.
/// </summary>
public interface IJwtTokenService
{
    /// <summary>
    /// Generate a JWT token for a user with standard claims (sub, email, tenant_id, roles, permissions).
    /// </summary>
    /// <param name="userId">The user's unique identifier</param>
    /// <param name="email">The user's email address</param>
    /// <param name="tenantId">The tenant the user belongs to</param>
    /// <param name="roles">The roles assigned to the user</param>
    /// <param name="permissions">The permissions granted to the user (aggregated from roles)</param>
    /// <returns>A JWT token string</returns>
    string GenerateUserToken(Guid userId, string email, Guid tenantId, IEnumerable<string> roles, IEnumerable<string>? permissions = null);

    /// <summary>
    /// Generate a JWT token for service-to-service communication.
    /// </summary>
    /// <param name="serviceName">The name of the calling service</param>
    /// <param name="tenantId">Optional tenant ID for tenant-scoped service calls</param>
    /// <param name="scopes">Optional scopes/permissions for the service</param>
    /// <returns>A JWT token string</returns>
    string GenerateServiceToken(string serviceName, Guid? tenantId = null, IEnumerable<string>? scopes = null);

    /// <summary>
    /// Validate a JWT token.
    /// </summary>
    /// <param name="token">The JWT token to validate</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>True if the token is valid, false otherwise</returns>
    Task<bool> ValidateTokenAsync(string token, CancellationToken ct = default);
}
