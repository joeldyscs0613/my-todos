namespace MyTodos.Services.IdentityService.Domain.PermissionAggregate.Contracts;

/// <summary>
/// Read repository for Permission aggregate.
/// </summary>
public interface IPermissionReadRepository : IReadRepository<Permission>
{
    /// <summary>
    /// Get permission by ID
    /// </summary>
    Task<Permission?> GetByIdAsync(Guid id, CancellationToken ct = default);

    /// <summary>
    /// Get permission by code
    /// </summary>
    Task<Permission?> GetByCodeAsync(string code, CancellationToken ct = default);

    /// <summary>
    /// Get all permissions
    /// </summary>
    Task<IReadOnlyList<Permission>> GetAllAsync(CancellationToken ct = default);

    /// <summary>
    /// Get permissions by IDs
    /// </summary>
    Task<IReadOnlyList<Permission>> GetByIdsAsync(IEnumerable<Guid> ids, CancellationToken ct = default);
}
