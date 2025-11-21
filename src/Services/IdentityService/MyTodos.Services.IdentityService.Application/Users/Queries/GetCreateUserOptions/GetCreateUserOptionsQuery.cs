using MyTodos.BuildingBlocks.Application.Abstractions.Dtos;
using MyTodos.BuildingBlocks.Application.Abstractions.Queries;
using MyTodos.BuildingBlocks.Application.Constants;
using MyTodos.BuildingBlocks.Application.Contracts.Security;
using MyTodos.Services.IdentityService.Application.Roles.Contracts;
using MyTodos.Services.IdentityService.Application.Tenants.Contracts;
using MyTodos.Services.IdentityService.Contracts;
using MyTodos.Services.IdentityService.Domain.RoleAggregate.Enums;
using MyTodos.SharedKernel.Helpers;

namespace MyTodos.Services.IdentityService.Application.Users.Queries.GetCreateUserOptions;

/// <summary>
/// Query to get dropdown options for user creation form.
/// Returns tenants, roles, and access scopes based on current user's permissions.
/// </summary>
public sealed class GetCreateUserOptionsQuery : Query<CreateUserOptionsResponseDto>
{
}

/// <summary>
/// Response DTO containing dropdown options for user creation.
/// </summary>
public sealed record CreateUserOptionsResponseDto
{
    /// <summary>
    /// Available tenants. Global.Admin sees all active tenants, others see only their own.
    /// </summary>
    public List<OptionDto<Guid>> Tenants { get; init; } = new();

    /// <summary>
    /// Available roles filtered by current user's scope.
    /// Global.Admin sees all, Tenant.Admin sees Tenant+App, others see App only.
    /// </summary>
    public List<RoleOptionDto> Roles { get; init; } = new();

    /// <summary>
    /// All access scope enum values (Global, Tenant, App).
    /// </summary>
    public List<OptionDto<AccessScope>> Scopes { get; init; } = new();

    /// <summary>
    /// Current user's access scope for UI context.
    /// </summary>
    public AccessScope CurrentUserScope { get; init; }
}

/// <summary>
/// We need the Scope to filter the role selection based on user's Scope selection
/// </summary>
/// <param name="Id"></param>
/// <param name="Name"></param>
/// <param name="Scope"></param>
public sealed record RoleOptionDto(Guid Id, string Name, AccessScope Scope) : OptionDto<Guid>(Id, Name);

/// <summary>
/// Handler for GetCreateUserOptionsQuery.
/// </summary>
public sealed class GetCreateUserOptionsQueryHandler
    : QueryHandler<GetCreateUserOptionsQuery, CreateUserOptionsResponseDto>
{
    private readonly ITenantPagedListReadRepository _tenantRepository;
    private readonly IRoleReadRepository _roleRepository;
    private readonly ICurrentUserService _currentUserService;

    public GetCreateUserOptionsQueryHandler(
        ITenantPagedListReadRepository tenantRepository,
        IRoleReadRepository roleRepository,
        ICurrentUserService currentUserService)
    {
        _tenantRepository = tenantRepository;
        _roleRepository = roleRepository;
        _currentUserService = currentUserService;
    }

    public override async Task<Result<CreateUserOptionsResponseDto>> Handle(
        GetCreateUserOptionsQuery request,
        CancellationToken ct)
    {
        // Determine current user's scope
        var isGlobalAdmin = _currentUserService.IsGlobalAdmin();
        var isTenantAdmin = _currentUserService.IsTenantAdmin();
        
        List<AccessScope> scopes = new();
        AccessScope currentUserScope;
        if (isGlobalAdmin)
        {
            currentUserScope = AccessScope.Global;
            scopes = Enum.GetValues<AccessScope>().Select(scope => scope).ToList();
        }
        else if (isTenantAdmin)
        {
            currentUserScope = AccessScope.Tenant;
            scopes = new List<AccessScope>()
            {
                AccessScope.Tenant, AccessScope.App
            };
        }
        else
        {
            currentUserScope = AccessScope.App;
            scopes = new List<AccessScope>()
            {
                AccessScope.App
            };
        }
        
        // Get tenants based on current user's scope
        List<OptionDto<Guid>> tenantOptions;
        if (isGlobalAdmin)
        {
            // Global.Admin sees all active tenants
            var allTenants = await _tenantRepository.GetAsOptionsAsync(ct);
            tenantOptions = allTenants
                .Where(t => t.IsActive)
                .Select(t => new OptionDto<Guid>(t.Id, t.Name))
                .OrderBy(t => t.Name)
                .ToList();
        }
        else
        {
            // Others see only their own tenant
            var currentTenantId = _currentUserService.TenantId;
            if (currentTenantId.HasValue)
            {
                var tenant = await _tenantRepository.GetByIdAsync(currentTenantId.Value, ct);
                if (tenant != null && tenant.IsActive)
                {
                    tenantOptions = new List<OptionDto<Guid>>
                    {
                        new(tenant.Id, tenant.Name)
                    };
                }
                else
                {
                    tenantOptions = new List<OptionDto<Guid>>();
                }
            }
            else
            {
                tenantOptions = new List<OptionDto<Guid>>();
            }
        }

        // Get roles based on current user's scope
        // scopes.Contains() translates properly because AccessScope has .HasConversion<int>() in RoleConfig
        var roles = await _roleRepository.GetAllAsync(r => scopes.Contains(r.Scope), ct);
        List<RoleOptionDto> roleOptions = roles
            .Select(r => new RoleOptionDto(r.Id, r.Name, r.Scope))
            .OrderBy(r => r.Scope)
            .ThenBy(r => r.Name)
            .ToList();
        
        // Get all access scope enum values
        var scopeOptions = scopes
            .Select(scope => new OptionDto<AccessScope>(scope, scope.ToString()))
            .OrderBy(s => s.Id)
            .ToList();

        var response = new CreateUserOptionsResponseDto
        {
            Tenants = tenantOptions,
            Roles = roleOptions,
            Scopes = scopeOptions,
            CurrentUserScope = currentUserScope
        };

        return Success(response);
    }
}
