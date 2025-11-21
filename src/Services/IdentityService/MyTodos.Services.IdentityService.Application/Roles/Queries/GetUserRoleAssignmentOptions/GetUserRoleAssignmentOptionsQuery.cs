using MyTodos.BuildingBlocks.Application.Abstractions.Queries;
using MyTodos.BuildingBlocks.Application.Constants;
using MyTodos.BuildingBlocks.Application.Contracts.Persistence;
using MyTodos.BuildingBlocks.Application.Contracts.Security;
using MyTodos.Services.IdentityService.Application.Roles.Contracts;
using MyTodos.Services.IdentityService.Contracts;
using MyTodos.Services.IdentityService.Domain.RoleAggregate.Enums;
using MyTodos.SharedKernel.Helpers;

namespace MyTodos.Services.IdentityService.Application.Roles.Queries.GetUserRoleAssignmentOptions;

/// <summary>
/// Query to get available roles that can be assigned to users.
/// Returns different roles based on the current user's scope:
/// - Global.Admin: Can see all roles
/// - Tenant.Admin: Can see only Tenant and App scope roles
/// - Others: Forbidden
/// </summary>
public sealed class GetUserRoleAssignmentOptionsQuery : Query<GetUserRoleAssignmentOptionsResponseDto>
{
}

/// <summary>
/// Response DTO containing available roles as options (id and name).
/// </summary>
public sealed record GetUserRoleAssignmentOptionsResponseDto
{
    public List<RoleOptionDto> Roles { get; init; } = new();
}

/// <summary>
/// Role option for dropdowns/selection lists.
/// </summary>
public sealed record RoleOptionDto
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Code { get; init; } = string.Empty;
    public string Scope { get; init; } = string.Empty;
}

/// <summary>
/// Handler for GetUserRoleAssignmentOptionsQuery.
/// </summary>
public sealed class GetUserRoleAssignmentOptionsQueryHandler
    : QueryHandler<GetUserRoleAssignmentOptionsQuery, GetUserRoleAssignmentOptionsResponseDto>
{
    private readonly IRoleReadRepository _roleRepository;
    private readonly ICurrentUserService _currentUserService;

    public GetUserRoleAssignmentOptionsQueryHandler(
        IRoleReadRepository roleRepository,
        ICurrentUserService currentUserService)
    {
        _roleRepository = roleRepository;
        _currentUserService = currentUserService;
    }

    public override async Task<Result<GetUserRoleAssignmentOptionsResponseDto>> Handle(
        GetUserRoleAssignmentOptionsQuery request,
        CancellationToken ct)
    {
        // Get current user's roles
        var isGlobalAdmin = _currentUserService.IsGlobalAdmin();
        var isTenantAdmin = _currentUserService.IsTenantAdmin();

        // Only Global.Admin and Tenant.Admin can assign roles
        if (!isGlobalAdmin && !isTenantAdmin)
        {
            return Forbidden("You do not have permission to assign roles");
        }

        // Get all roles
        var allRoles = await _roleRepository.GetAllAsync(null, ct);

        // Filter based on current user's scope
        List<RoleOptionDto> availableRoles;

        if (isGlobalAdmin)
        {
            // Global admins can assign ANY role
            availableRoles = allRoles
                .Select(r => new RoleOptionDto
                {
                    Id = r.Id,
                    Name = r.Name,
                    Code = r.Code,
                    Scope = r.Scope.ToString()
                })
                .OrderBy(r => r.Scope)
                .ThenBy(r => r.Name)
                .ToList();
        }
        else // isTenantAdmin
        {
            // Tenant admins can only assign Tenant and App scope roles
            availableRoles = allRoles
                .Where(r => r.Scope == AccessScope.Tenant || r.Scope == AccessScope.App)
                .Select(r => new RoleOptionDto
                {
                    Id = r.Id,
                    Name = r.Name,
                    Code = r.Code,
                    Scope = r.Scope.ToString()
                })
                .OrderBy(r => r.Scope)
                .ThenBy(r => r.Name)
                .ToList();
        }

        var response = new GetUserRoleAssignmentOptionsResponseDto
        {
            Roles = availableRoles
        };

        return Success(response);
    }
}
