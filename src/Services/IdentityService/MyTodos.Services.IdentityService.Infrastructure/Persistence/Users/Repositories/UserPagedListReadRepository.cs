using Microsoft.EntityFrameworkCore;
using MyTodos.BuildingBlocks.Infrastructure.Persistence.Abstractions.Repositories;
using MyTodos.Services.IdentityService.Application.Users;
using MyTodos.Services.IdentityService.Application.Users.Contracts;
using MyTodos.Services.IdentityService.Application.Users.Queries;
using MyTodos.Services.IdentityService.Application.Users.Queries.GetPagedList;
using MyTodos.Services.IdentityService.Domain.UserAggregate;
using MyTodos.Services.IdentityService.Infrastructure.Persistence;

namespace MyTodos.Services.IdentityService.Infrastructure.UserAggregate.Repositories;

/// <summary>
/// Read-only repository for User aggregate queries.
/// </summary>
public sealed class UserPagedListReadRepository(IdentityServiceDbContext context)
    : PagedListReadEfRepository<User, Guid, UserPagedListSpecification, UserPagedListFilter, IdentityServiceDbContext>(context, new UserQueryConfiguration())
        , IUserPagedListReadRepository
{
    public async Task<User?> GetByUsernameAsync(string username, CancellationToken ct = default)
        => await GetFirstOrDefaultAsync(u => u.Username == username, ct);

    public async Task<User?> GetByEmailAsync(string email, CancellationToken ct = default)
        => await GetFirstOrDefaultAsync(u => u.Email == email, ct);

    public async Task<IReadOnlyList<User>> GetByTenantIdAsync(Guid tenantId, CancellationToken ct = default)
        => await GetAllAsync(u => u.UserRoles.Any(ur => ur.TenantId == tenantId), ct);

    #region UserInvitations
    
    public async Task<UserInvitation?> GetUserInvitationByTokenAsync(string token, CancellationToken ct = default)
        => await Context.UserInvitations.FirstOrDefaultAsync(ui => ui.InvitationToken == token, ct);

    public async Task<IReadOnlyList<UserInvitation>> GetUserInvitationsPendingByEmailAsync(
        string email, CancellationToken ct = default)
        => await Context.UserInvitations.Where(ui => ui.Email == email).ToListAsync(ct);

    public async Task<IReadOnlyList<UserInvitation>> GetUserInvitationsPendingByTenantIdAsync(
        Guid tenantId, CancellationToken ct = default)
        => await Context.UserInvitations.Where(ui => ui.TenantId == tenantId 
            && ui.Status == Domain.UserAggregate.Enums.InvitationStatus.Pending).ToListAsync(ct);

    public async Task<bool> UserInvitationExistsForEmailAsync(string email, CancellationToken ct = default)
        => await Context.UserInvitations.AnyAsync(ui => ui.Email == email, ct);
    
    #endregion
}
