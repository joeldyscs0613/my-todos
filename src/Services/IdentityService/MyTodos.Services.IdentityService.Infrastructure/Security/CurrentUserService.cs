using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using MyTodos.BuildingBlocks.Application.Contracts.Security;

namespace MyTodos.Services.IdentityService.Infrastructure.Security;

/// <summary>
/// Retrieves current user information from HTTP context claims.
/// Claims are parsed once during construction for better performance.
/// </summary>
public sealed class CurrentUserService : ICurrentUserService
{
    public Guid? UserId { get; }
    public string? Username { get; }
    public Guid? TenantId { get; }
    public bool IsAuthenticated { get; }

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        var user = httpContextAccessor.HttpContext?.User;

        IsAuthenticated = user?.Identity?.IsAuthenticated ?? false;

        if (IsAuthenticated)
        {
            var userIdClaim = user!.FindFirstValue(ClaimTypes.NameIdentifier);
            UserId = Guid.TryParse(userIdClaim, out var userId) ? userId : null;

            Username = user.FindFirstValue(ClaimTypes.Name);

            var tenantIdClaim = user.FindFirstValue("tenant_id");
            TenantId = Guid.TryParse(tenantIdClaim, out var tenantId) ? tenantId : null;
        }
    }
}
