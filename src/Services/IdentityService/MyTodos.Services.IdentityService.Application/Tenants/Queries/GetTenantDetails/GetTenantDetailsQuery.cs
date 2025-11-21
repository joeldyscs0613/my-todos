using MyTodos.BuildingBlocks.Application.Abstractions.Queries;
using MyTodos.BuildingBlocks.Application.Contracts.Security;
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
    public bool IsActive { get; init; }

    public DateTimeOffset CreatedDate { get; set; }
    public string? CreatedBy { get; set; }

    public DateTimeOffset? ModifiedDate { get; set; }
    public string? ModifiedBy { get; set; }
}

public sealed class GetTenantDetailsQueryHandler(
    ITenantPagedListReadRepository readRepository,
    ICurrentUserService currentUserService)
    : QueryHandler<GetTenantDetailsQuery, TenantDetailsResponseDto>
{
    public override async Task<Result<TenantDetailsResponseDto>> Handle(GetTenantDetailsQuery request, CancellationToken ct)
    {
        // Only Global Administrators can view tenant details
        if (!currentUserService.IsGlobalAdmin())
        {
            return Forbidden("Only Global Administrators can view tenant details");
        }

        var tenant = await readRepository.GetByIdAsync(request.TenantId, ct);

        if (tenant is null)
        {
            return NotFound($"Tenant with ID '{request.TenantId}' not found.");
        }

        var dto = new TenantDetailsResponseDto
        {
            Id = tenant.Id,
            Name = tenant.Name,
            IsActive = tenant.IsActive,
            CreatedDate = tenant.CreatedDate,
            CreatedBy = tenant.CreatedBy,
            ModifiedDate = tenant.ModifiedDate,
            ModifiedBy = tenant.ModifiedBy,
        };

        return Success(dto);
    }
}
