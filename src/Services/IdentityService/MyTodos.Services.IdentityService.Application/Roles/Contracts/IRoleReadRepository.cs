namespace MyTodos.Services.IdentityService.Domain.RoleAggregate.Contracts;

/// <summary>
/// Read repository for Role aggregate.
/// </summary>
public interface IRoleReadRepository
{
    /// <summary>
    /// Get role by ID
    /// </summary>
    Task<Role?> GetByIdAsync(Guid id, CancellationToken ct = default);

    /// <summary>
    /// Get role by ID with permissions loaded
    /// </summary>
    Task<Role?> GetByIdWithPermissionsAsync(Guid id, CancellationToken ct = default);

    /// <summary>
    /// Get role by code
    /// </summary>
    Task<Role?> GetByCodeAsync(string code, CancellationToken ct = default);

    /// <summary>
    /// Get all roles
    /// </summary>
    Task<IReadOnlyList<Role>> GetAllAsync(CancellationToken ct = default);
}
