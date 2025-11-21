using MyTodos.BuildingBlocks.Application.Abstractions.Queries;
using MyTodos.Services.IdentityService.Application.Roles.Contracts;
using MyTodos.Services.IdentityService.Domain.RoleAggregate.Enums;
using MyTodos.SharedKernel.Helpers;

namespace MyTodos.Services.IdentityService.Application.Roles.Queries.GetRoleDetails;

/// <summary>
/// Query to get role details by ID.
/// </summary>
public sealed class GetRoleDetailsQuery(Guid roleId) : Query<RoleDetailsResponseDto>
{
    public Guid RoleId { get; init; } = roleId;
}

/// <summary>
/// Response DTO for role details.
/// </summary>
public sealed record RoleDetailsResponseDto
{
    public Guid Id { get; init; }
    public string Code { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public string? Description { get; init; }
    public AccessScope Scope { get; init; }

    public DateTimeOffset CreatedDate { get; set; }
    public string? CreatedBy { get; set; }

    public DateTimeOffset? ModifiedDate { get; set; }
    public string? ModifiedBy { get; set; }

    public List<PermissionForRoleDetailsResponseDto> Permissions { get; set; } = new();
}

/// <summary>
/// Response DTO for permission details.
/// </summary>
public sealed record PermissionForRoleDetailsResponseDto
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
}

/// <summary>
/// Handler for getting role details.
/// </summary>
public sealed class GetRoleDetailsQueryHandler(IRoleReadRepository readRepository)
    : QueryHandler<GetRoleDetailsQuery, RoleDetailsResponseDto>
{
    public override async Task<Result<RoleDetailsResponseDto>> Handle(GetRoleDetailsQuery request, CancellationToken ct)
    {
        var role = await readRepository.GetByIdAsync(request.RoleId, ct);

        if (role is null)
        {
            return NotFound($"Role with ID '{request.RoleId}' not found.");
        }

        var permissions = new List<PermissionForRoleDetailsResponseDto>();
        
        foreach (var rolePermission in role.RolePermissions)
        {
            if (rolePermission.Permission != null)
            {
                permissions.Add(new PermissionForRoleDetailsResponseDto
                {
                    Id = rolePermission.Permission.Id,
                    Name = rolePermission.Permission.Name,
                });
            }
        }
        
        var dto = new RoleDetailsResponseDto
        {
            Id = role.Id,
            Code = role.Code,
            Name = role.Name,
            Description = role.Description,
            Scope = role.Scope,
            CreatedDate = role.CreatedDate,
            CreatedBy = role.CreatedBy,
            ModifiedDate = role.ModifiedDate,
            ModifiedBy = role.ModifiedBy,
            Permissions = permissions
        };

        return Success(dto);
    }
}
