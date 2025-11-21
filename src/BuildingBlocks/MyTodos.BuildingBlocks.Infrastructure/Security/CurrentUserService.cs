using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using MyTodos.BuildingBlocks.Application.Contracts.Security;

namespace MyTodos.BuildingBlocks.Infrastructure.Security;

/// <summary>
/// Base implementation for retrieving current user information from HTTP context claims.
/// Claims are parsed once during construction for better performance.
/// Services can inherit from this class to add custom methods while reusing core functionality.
/// </summary>
public class CurrentUserService : ICurrentUserService
{
    protected readonly ClaimsPrincipal? _user;

    public Guid? UserId { get; }
    public string? Username { get; }
    public Guid? TenantId { get; }
    public bool IsAuthenticated { get; }

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _user = httpContextAccessor.HttpContext?.User;

        IsAuthenticated = _user?.Identity?.IsAuthenticated ?? false;

        if (IsAuthenticated)
        {
            var userIdClaim = _user!.FindFirstValue(ClaimTypes.NameIdentifier);
            UserId = Guid.TryParse(userIdClaim, out var userId) ? userId : null;

            Username = _user.FindFirstValue(ClaimTypes.Name);

            var tenantIdClaim = _user.FindFirstValue("tenant_id");
            TenantId = Guid.TryParse(tenantIdClaim, out var tenantId) ? tenantId : null;
        }
    }

    /// <summary>
    /// Gets the list of role names/codes from user claims.
    /// Virtual to allow services to override if they need custom role extraction logic.
    /// </summary>
    public virtual List<string> GetRoles()
    {
        if (_user == null || !IsAuthenticated)
            return new List<string>();

        return _user.FindAll(ClaimTypes.Role)
            .Select(c => c.Value)
            .ToList();
    }

    /// <summary>
    /// Gets the list of permission codes from user claims.
    /// Virtual to allow services to override if they need custom permission extraction logic.
    /// </summary>
    public virtual List<string> GetPermissions()
    {
        if (_user == null || !IsAuthenticated)
            return new List<string>();

        return _user.FindAll("permission")
            .Select(c => c.Value)
            .ToList();
    }
}
