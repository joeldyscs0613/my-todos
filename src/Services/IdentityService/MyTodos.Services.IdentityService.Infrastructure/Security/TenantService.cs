using MyTodos.BuildingBlocks.Application.Contracts.Security;
using MyTodos.Services.IdentityService.Application.Users.Contracts;

namespace MyTodos.Services.IdentityService.Infrastructure.Security;

/// <summary>
/// Tenant resolution and validation service.
/// </summary>
public sealed class TenantService(
    ICurrentUserService currentUserService,
    IUserPagedListReadRepository userPagedListReadRepository)
    : ITenantService
{
    public Guid? GetCurrentTenantId()
    {
        return currentUserService.TenantId;
    }

    public async Task<bool> ValidateTenantAccessAsync(Guid tenantId, CancellationToken ct = default)
    {
        // Not authenticated - no access
        if (!currentUserService.IsAuthenticated || !currentUserService.UserId.HasValue)
            return false;

        // Get user with roles to check tenant access
        var user = await userPagedListReadRepository.GetByIdAsync(currentUserService.UserId.Value, ct);
        if (user == null)
            return false;

        // Check if user has any role for the specified tenant (or global role)
        return user.UserRoles.Any(ur =>
            ur.TenantId == tenantId ||  // Tenant-specific role
            !ur.TenantId.HasValue);     // Global role (has access to all tenants)
    }
}
