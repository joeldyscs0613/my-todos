using MyTodos.BuildingBlocks.Application.Contracts.Persistence;
using MyTodos.Services.IdentityService.Application.Tenants.Queries.GetPagedList;
using MyTodos.Services.IdentityService.Domain.TenantAggregate;

namespace MyTodos.Services.IdentityService.Application.Tenants.Contracts;

/// <summary>
/// Read repository for Tenant aggregate.
/// </summary>
public interface ITenantPagedListReadRepository 
    : IPagedListReadRepository<Tenant, Guid, TenantPagedListSpecification, TenantPagedListFilter>
{
    /// <summary>
    /// Get tenant by name
    /// </summary>
    Task<Tenant?> GetByNameAsync(string name, CancellationToken ct = default);
}
