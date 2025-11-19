using MyTodos.BuildingBlocks.Application.Contracts.Security;
using MyTodos.BuildingBlocks.Infrastructure.Persistence.Abstractions.Repositories;
using MyTodos.Services.IdentityService.Application.Tenants.Contracts;
using MyTodos.Services.IdentityService.Application.Tenants.Queries.GetPagedList;
using MyTodos.Services.IdentityService.Domain.TenantAggregate;
using MyTodos.Services.IdentityService.Infrastructure.Persistence;

namespace MyTodos.Services.IdentityService.Infrastructure.TenantAggregate.Repositories;

/// <summary>
/// Read-only repository for Tenant aggregate queries.
/// </summary>
public sealed class TenantPagedListReadRepository(IdentityServiceDbContext context, ICurrentUserService currentUserService)
    : PagedListReadEfRepository<Tenant, Guid, TenantPagedListSpecification, TenantPagedListFilter,
            IdentityServiceDbContext>(context, new TenantQueryConfiguration(), currentUserService)
        , ITenantPagedListReadRepository
{
    public async Task<Tenant?> GetByNameAsync(string name, CancellationToken ct = default)
        => await GetFirstOrDefaultAsync(t => t.Name == name, ct);
}
