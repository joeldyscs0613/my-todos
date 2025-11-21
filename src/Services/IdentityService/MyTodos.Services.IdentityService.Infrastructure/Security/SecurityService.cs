using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using MyTodos.BuildingBlocks.Application.Contracts.Security;

namespace MyTodos.Services.IdentityService.Infrastructure.Security;

/// <summary>
/// Extended security service with roles and permissions.
/// </summary>
public sealed class SecurityService : ISecurityService
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly Lazy<IReadOnlyList<string>> _roles;
    private readonly Lazy<IReadOnlyList<string>> _permissions;

    public SecurityService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
        _roles = new Lazy<IReadOnlyList<string>>(LoadRoles);
        _permissions = new Lazy<IReadOnlyList<string>>(LoadPermissions);
    }

    public Guid? UserId
    {
        get
        {
            var userIdClaim = _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);
            return Guid.TryParse(userIdClaim, out var userId) ? userId : null;
        }
    }

    public string? Username
    {
        get
        {
            return _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.Name);
        }
    }

    public string? Email
    {
        get
        {
            return _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.Email);
        }
    }

    public Guid? TenantId
    {
        get
        {
            var tenantIdClaim = _httpContextAccessor.HttpContext?.User?.FindFirstValue("tenant_id");
            return Guid.TryParse(tenantIdClaim, out var tenantId) ? tenantId : null;
        }
    }

    public bool IsAuthenticated
    {
        get
        {
            return _httpContextAccessor.HttpContext?.User?.Identity?.IsAuthenticated ?? false;
        }
    }

    public IReadOnlyList<string> Roles => _roles.Value;

    public bool IsInRole(string role)
    {
        if (string.IsNullOrWhiteSpace(role))
            return false;

        return Roles.Contains(role, StringComparer.OrdinalIgnoreCase);
    }

    public bool HasPermission(string permission)
    {
        if (string.IsNullOrWhiteSpace(permission))
            return false;

        // Check for wildcard permission (Global Admin)
        if (_permissions.Value.Contains("*"))
            return true;

        // Check for exact permission match
        if (_permissions.Value.Contains(permission, StringComparer.OrdinalIgnoreCase))
            return true;

        // Check for wildcard namespace match (e.g., "users.*" matches "users.create")
        var parts = permission.Split('.');
        if (parts.Length > 1)
        {
            var namespaceWildcard = $"{parts[0]}.*";
            if (_permissions.Value.Contains(namespaceWildcard, StringComparer.OrdinalIgnoreCase))
                return true;
        }

        return false;
    }

    private IReadOnlyList<string> LoadRoles()
    {
        var roleClaims = _httpContextAccessor.HttpContext?.User?.FindAll(ClaimTypes.Role);
        return roleClaims?.Select(c => c.Value).ToList() ?? new List<string>();
    }

    private IReadOnlyList<string> LoadPermissions()
    {
        var permissionClaims = _httpContextAccessor.HttpContext?.User?.FindAll("permission");
        return permissionClaims?.Select(c => c.Value).ToList() ?? new List<string>();
    }

    public List<string> GetRoles()
    {
        return Roles.ToList();
    }

    public List<string> GetPermissions()
    {
        return _permissions.Value.ToList();
    }

    public bool IsGlobalAdmin()
    {
        return IsInRole("Global.Admin");
    }

    public bool IsTenantAdmin()
    {
        return IsInRole("Tenant.Admin");
    }
}
