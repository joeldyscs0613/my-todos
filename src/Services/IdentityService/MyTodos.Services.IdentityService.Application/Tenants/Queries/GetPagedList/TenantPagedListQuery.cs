using MyTodos.BuildingBlocks.Application.Abstractions.Queries;
using MyTodos.Services.IdentityService.Application.Tenants.Contracts;
using MyTodos.Services.IdentityService.Domain.TenantAggregate;
using MyTodos.Services.IdentityService.Domain.TenantAggregate.Enums;

namespace MyTodos.Services.IdentityService.Application.Tenants.Queries.GetPagedList;

public sealed class TenantPagedListQuery 
    : PagedListQuery<TenantPagedListSpecification, TenantPagedListFilter, TenantPagedListResponseDto>
{
}

public sealed class TenantPagedListQueryHandler(ITenantPagedListReadRepository readRepository)
    : PagedListQueryHandler<Tenant, Guid, TenantPagedListSpecification, TenantPagedListFilter,
        TenantPagedListQuery, TenantPagedListResponseDto>(readRepository)
{
    protected override List<TenantPagedListResponseDto> GetResultList(
        TenantPagedListQuery request, IReadOnlyList<Tenant> list)
    {
        return list.Select(t
            => new TenantPagedListResponseDto(t.Id, t.Name, Enum.GetName(typeof(TenantPlan), t.Plan), t.IsActive))
            .ToList();
    }
}