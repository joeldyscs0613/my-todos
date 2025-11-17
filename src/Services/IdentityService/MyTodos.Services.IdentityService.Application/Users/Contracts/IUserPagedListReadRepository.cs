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

    #region UserInvitations

    /// <summary>
    /// Get invitation by token
    /// </summary>
    Task<UserInvitation?> GetUserInvitationByTokenAsync(string token, CancellationToken ct = default);

    /// <summary>
    /// Get pending invitations for a specific email
    /// </summary>
    Task<IReadOnlyList<UserInvitation>> GetUserInvitationsPendingByEmailAsync(
        string email, CancellationToken ct = default);

    /// <summary>
    /// Get all pending invitations for a tenant
    /// </summary>
    Task<IReadOnlyList<UserInvitation>> GetUserInvitationsPendingByTenantIdAsync(
        Guid tenantId, CancellationToken ct = default);

    /// <summary>
    /// Check if an invitation exists for an email
    /// </summary>
    Task<bool> UserInvitationExistsForEmailAsync(string email, CancellationToken ct = default);

    #endregion
}
