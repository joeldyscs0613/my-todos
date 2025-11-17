using MyTodos.BuildingBlocks.Infrastructure.Persistence.Abstractions.Repositories;
using MyTodos.Services.IdentityService.Application.Users.Contracts;
using MyTodos.Services.IdentityService.Application.Users.Queries;
using MyTodos.Services.IdentityService.Application.Users.Queries.GetPagedList;
using MyTodos.Services.IdentityService.Domain.UserAggregate;
using MyTodos.Services.IdentityService.Infrastructure.Persistence;

namespace MyTodos.Services.IdentityService.Infrastructure.UserAggregate.Repositories;

/// <summary>
/// Write repository for User aggregate mutations.
/// </summary>
public sealed class UserWriteRepository(IdentityServiceDbContext context)
    : WriteEfRepository<User, Guid, IdentityServiceDbContext>(context, new UserQueryConfiguration()),
        IUserWriteRepository
{
    #region UserInvitations
    
    public async Task AddUserInvitationAsync(UserInvitation invitation, CancellationToken ct = default)
    {
        await Context.UserInvitations.AddAsync(invitation, ct);
    }

    public Task UpdateUserInvitationAsync(UserInvitation invitation, CancellationToken ct = default)
    {
        Context.UserInvitations.Update(invitation);
        
        return Task.CompletedTask;
    }

    public Task DeleteUserInvitationAsync(UserInvitation invitation, CancellationToken ct = default)
    {
        Context.UserInvitations.Remove(invitation);
        
        return Task.CompletedTask;
    }
    
    #endregion
}
