using MyTodos.BuildingBlocks.Application.Abstractions.Queries;
using MyTodos.Services.IdentityService.Application.Tenants.Contracts;
using MyTodos.SharedKernel.Helpers;

namespace MyTodos.Services.IdentityService.Application.Tenants.Queries.GetTenantDetails;

public sealed class GetTenantDetailsQueryHandler(ITenantPagedListReadRepository readRepository)
    : QueryHandler<GetTenantDetailsQuery, TenantDetailsDto>
{
    public override async Task<Result<TenantDetailsDto>> Handle(GetTenantDetailsQuery request, CancellationToken ct)
    {
        var tenant = await readRepository.GetByIdAsync(request.TenantId, ct);

        if (tenant is null)
        {
            return NotFound($"Tenant with ID '{request.TenantId}' not found.");
        }

        var dto = new TenantDetailsDto
        {
            Id = tenant.Id,
            Name = tenant.Name,
            Plan = tenant.Plan.ToString(),
            MaxUsers = tenant.MaxUsers,
            IsActive = tenant.IsActive,
            SubscriptionExpiresAt = tenant.SubscriptionExpiresAt
        };

        return Success(dto);
    }
}
