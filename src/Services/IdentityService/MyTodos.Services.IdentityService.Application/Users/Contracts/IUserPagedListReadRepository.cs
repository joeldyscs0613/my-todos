using MyTodos.BuildingBlocks.Application.Contracts.Persistence;
using MyTodos.Services.IdentityService.Application.Users.Queries;
using MyTodos.Services.IdentityService.Application.Users.Queries.GetPagedList;
using MyTodos.Services.IdentityService.Domain.UserAggregate;

namespace MyTodos.Services.IdentityService.Application.Users.Contracts;

/// <summary>
/// Read repository for User aggregate.
/// </summary>
public interface IUserPagedListReadRepository
    : IPagedListReadRepository<User, Guid, UserPagedListSpecification, UserPagedListFilter>
{
    /// <summary>
    /// Get user by username
    /// </summary>
    Task<User?> GetByUsernameAsync(string username, CancellationToken ct = default);

    /// <summary>
    /// Get user by email
    /// </summary>
    Task<User?> GetByEmailAsync(string email, CancellationToken ct = default);

    /// <summary>
    /// Get all users in a specific tenant
    /// </summary>
    Task<IReadOnlyList<User>> GetByTenantIdAsync(Guid tenantId, CancellationToken ct = default);
}
