using MyTodos.BuildingBlocks.Application.Abstractions.Queries;
using MyTodos.Services.IdentityService.Application.Permissions.Contracts;
using MyTodos.SharedKernel.Helpers;

namespace MyTodos.Services.IdentityService.Application.Permissions.Queries.GetPermissionDetails;

/// <summary>
/// Query to get permission details by ID.
/// </summary>
public sealed class GetPermissionDetailsQuery(Guid permissionId) : Query<PermissionDetailsResponseDto>
{
    public Guid PermissionId { get; init; } = permissionId;
}

/// <summary>
/// Response DTO for permission details.
/// </summary>
public sealed record PermissionDetailsResponseDto
{
    public Guid Id { get; init; }
    public string Code { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public string? Description { get; init; }

    public DateTimeOffset CreatedDate { get; set; }
    public string? CreatedBy { get; set; }

    public DateTimeOffset? ModifiedDate { get; set; }
    public string? ModifiedBy { get; set; }
}

/// <summary>
/// Handler for getting permission details.
/// </summary>
public sealed class GetPermissionDetailsQueryHandler(IPermissionReadRepository readRepository)
    : QueryHandler<GetPermissionDetailsQuery, PermissionDetailsResponseDto>
{
    public override async Task<Result<PermissionDetailsResponseDto>> Handle(GetPermissionDetailsQuery request, CancellationToken ct)
    {
        var permission = await readRepository.GetByIdAsync(request.PermissionId, ct);

        if (permission is null)
        {
            return NotFound($"Permission with ID '{request.PermissionId}' not found.");
        }

        var dto = new PermissionDetailsResponseDto
        {
            Id = permission.Id,
            Code = permission.Code,
            Name = permission.Name,
            Description = permission.Description,
            CreatedDate = permission.CreatedDate,
            CreatedBy = permission.CreatedBy,
            ModifiedDate = permission.ModifiedDate,
            ModifiedBy = permission.ModifiedBy,
        };

        return Success(dto);
    }
}
