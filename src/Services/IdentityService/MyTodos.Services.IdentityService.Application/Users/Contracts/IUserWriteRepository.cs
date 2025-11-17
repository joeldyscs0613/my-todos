namespace MyTodos.Services.IdentityService.Domain.UserAggregate.Contracts;

/// <summary>
/// Write repository for User aggregate.
/// </summary>
public interface IUserWriteRepository
{
    /// <summary>
    /// Add a new user
    /// </summary>
    Task AddAsync(User user, CancellationToken ct = default);

    /// <summary>
    /// Update an existing user
    /// </summary>
    Task UpdateAsync(User user, CancellationToken ct = default);

    /// <summary>
    /// Delete a user
    /// </summary>
    Task DeleteAsync(User user, CancellationToken ct = default);
}
