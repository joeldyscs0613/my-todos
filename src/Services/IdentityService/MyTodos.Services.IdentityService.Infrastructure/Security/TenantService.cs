using MyTodos.BuildingBlocks.Application.Contracts.Security;
using MyTodos.Services.IdentityService.Domain.UserAggregate.Contracts;

namespace MyTodos.Services.IdentityService.Infrastructure.Security;

/// <summary>
/// Tenant resolution and validation service.
/// </summary>
public sealed class TenantService : ITenantService
{
    private readonly ICurrentUserService _currentUserService;
    private readonly IUserReadRepository _userReadRepository;

    public TenantService(
        ICurrentUserService currentUserService,
        IUserReadRepository userReadRepository)
    {
        _currentUserService = currentUserService;
        _userReadRepository = userReadRepository;
    }

    public Guid? GetCurrentTenantId()
    {
        return _currentUserService.TenantId;
    }

    public async Task<bool> ValidateTenantAccessAsync(Guid tenantId, CancellationToken ct = default)
    {
        // Not authenticated - no access
        if (!_currentUserService.IsAuthenticated || !_currentUserService.UserId.HasValue)
            return false;

        // Get user with roles to check tenant access
        var user = await _userReadRepository.GetByIdWithRolesAsync(_currentUserService.UserId.Value, ct);
        if (user == null)
            return false;

        // Check if user has any role for the specified tenant (or global role)
        return user.UserRoles.Any(ur =>
            ur.TenantId == tenantId ||  // Tenant-specific role
            !ur.TenantId.HasValue);     // Global role (has access to all tenants)
    }
}
