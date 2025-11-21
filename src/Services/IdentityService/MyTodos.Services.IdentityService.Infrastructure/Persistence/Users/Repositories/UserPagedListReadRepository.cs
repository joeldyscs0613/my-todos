using MyTodos.BuildingBlocks.Application.Contracts.Security;
using MyTodos.BuildingBlocks.Infrastructure.Persistence.Abstractions.Repositories;
using MyTodos.Services.IdentityService.Application.Users.Contracts;
using MyTodos.Services.IdentityService.Application.Users.Queries.GetPagedList;
using MyTodos.Services.IdentityService.Domain.UserAggregate;

namespace MyTodos.Services.IdentityService.Infrastructure.Persistence.Users.Repositories;

/// <summary>
/// Read-only repository for User aggregate queries.
/// Enforces tenant isolation: Tenant admins can only see users in their tenant.
/// </summary>
public sealed class UserPagedListReadRepository(IdentityServiceDbContext context, ICurrentUserService currentUserService)
    : PagedListReadEfRepository<User, Guid, UserPagedListSpecification, UserPagedListFilter, IdentityServiceDbContext>(context, new UserQueryConfiguration(), currentUserService)
        , IUserPagedListReadRepository
{
    private readonly ICurrentUserService _currentUserService = currentUserService;

    /// <summary>
    /// Applies tenant isolation for list queries.
    /// Global admins see all users. Tenant admins only see users in their tenant.
    /// </summary>
    protected override IQueryable<User> GetInitialQueryForList()
    {
        var query = base.GetInitialQueryForList();

        // If current user is a tenant admin (has a tenant ID), filter to only that tenant
        var currentUserTenantId = _currentUserService.TenantId;
        if (currentUserTenantId.HasValue)
        {
            // Tenant admins can only see users that have at least one role in their tenant
            query = query.Where(u => u.UserRoles.Any(ur => ur.TenantId == currentUserTenantId.Value));
        }
        // Global admins (no tenant ID) can see all users

        return query;
    }

    public async Task<User?> GetByUsernameAsync(string username, CancellationToken ct = default)
        => await GetFirstOrDefaultAsync(u => u.Username == username, ct);

    public async Task<User?> GetByEmailAsync(string email, CancellationToken ct = default)
        => await GetFirstOrDefaultAsync(u => u.Email == email, ct);

    public async Task<IReadOnlyList<User>> GetByTenantIdAsync(Guid tenantId, CancellationToken ct = default)
        => await GetAllAsync(u => u.UserRoles.Any(ur => ur.TenantId == tenantId), ct);
}
