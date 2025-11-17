using MyTodos.BuildingBlocks.Application.Contracts.Persistence;
using MyTodos.Services.IdentityService.Domain.UserAggregate;

namespace MyTodos.Services.IdentityService.Application.Users.Contracts;

/// <summary>
/// Write repository for User aggregate.
/// </summary>
public interface IUserWriteRepository : IWriteRepository<User, Guid>
{
    #region UserInvitations

    Task AddUserInvitationAsync(UserInvitation invitation, CancellationToken ct = default);

    /// <summary>
    /// Update an existing invitation
    /// </summary>
    Task UpdateUserInvitationAsync(UserInvitation invitation, CancellationToken ct = default);

    /// <summary>
    /// Delete an invitation
    /// </summary>
    Task DeleteUserInvitationAsync(UserInvitation invitation, CancellationToken ct = default);

    #endregion
}
