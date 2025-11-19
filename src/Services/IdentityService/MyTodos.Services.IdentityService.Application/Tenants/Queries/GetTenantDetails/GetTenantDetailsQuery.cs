using MyTodos.BuildingBlocks.Application.Abstractions.Queries;
using MyTodos.Services.IdentityService.Application.Tenants.Contracts;
using MyTodos.SharedKernel.Helpers;

namespace MyTodos.Services.IdentityService.Application.Tenants.Queries.GetTenantDetails;

public sealed class GetTenantDetailsQuery(Guid tenantId) : Query<TenantDetailsResponseDto>
{
    public Guid TenantId { get; init; } = tenantId;
}

public sealed record TenantDetailsResponseDto
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Plan { get; init; } = string.Empty;
    public int MaxUsers { get; init; }
    public bool IsActive { get; init; }
    public DateTimeOffset? SubscriptionExpiresAt { get; init; }
}

public sealed class GetTenantDetailsQueryHandler(ITenantPagedListReadRepository readRepository)
    : QueryHandler<GetTenantDetailsQuery, TenantDetailsResponseDto>
{
    public override async Task<Result<TenantDetailsResponseDto>> Handle(GetTenantDetailsQuery request, CancellationToken ct)
    {
        var tenant = await readRepository.GetByIdAsync(request.TenantId, ct);

        if (tenant is null)
        {
            return NotFound($"Tenant with ID '{request.TenantId}' not found.");
        }

        var dto = new TenantDetailsResponseDto
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
