namespace MyTodos.Services.IdentityService.Domain.RoleAggregate.Contracts;

/// <summary>
/// Write repository for Role aggregate.
/// </summary>
public interface IRoleWriteRepository
{
    /// <summary>
    /// Add a new role
    /// </summary>
    Task AddAsync(Role role, CancellationToken ct = default);

    /// <summary>
    /// Update an existing role
    /// </summary>
    Task UpdateAsync(Role role, CancellationToken ct = default);

    /// <summary>
    /// Delete a role
    /// </summary>
    Task DeleteAsync(Role role, CancellationToken ct = default);
}
