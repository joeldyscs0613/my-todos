namespace MyTodos.Services.TodoService.Application.Shared.Contracts;

/// <summary>
/// Repository for fetching user options for task/project assignment
/// </summary>
public interface IAssigneeOptionsRepository
{
    /// <summary>
    /// Get all active users in a tenant for assignment dropdown
    /// </summary>
    Task<IReadOnlyList<UserOption>> GetUsersByTenantIdAsync(Guid tenantId, CancellationToken ct = default);
}

/// <summary>
/// Lightweight user representation for assignment options
/// </summary>
public sealed record UserOption(Guid Id, string FullName);
