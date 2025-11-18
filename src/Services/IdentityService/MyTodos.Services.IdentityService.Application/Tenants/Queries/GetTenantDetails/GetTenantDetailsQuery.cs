using MyTodos.BuildingBlocks.Application.Abstractions.Queries;

namespace MyTodos.Services.IdentityService.Application.Tenants.Queries.GetTenantDetails;

public sealed class GetTenantDetailsQuery(Guid tenantId) : Query<TenantDetailsDto>
{
    public Guid TenantId { get; init; } = tenantId;
}
