namespace MyTodos.Services.IdentityService.Domain.PermissionAggregate.Contracts;

/// <summary>
/// Write repository for Permission aggregate.
/// </summary>
public interface IPermissionWriteRepository
{
    /// <summary>
    /// Add a new permission
    /// </summary>
    Task AddAsync(Permission permission, CancellationToken ct = default);

    /// <summary>
    /// Update an existing permission
    /// </summary>
    Task UpdateAsync(Permission permission, CancellationToken ct = default);

    /// <summary>
    /// Delete a permission
    /// </summary>
    Task DeleteAsync(Permission permission, CancellationToken ct = default);
}
