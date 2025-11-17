using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using MyTodos.BuildingBlocks.Application.Contracts.Security;

namespace MyTodos.Services.IdentityService.Infrastructure.Security;

/// <summary>
/// Retrieves current user information from HTTP context claims.
/// </summary>
public sealed class CurrentUserService(IHttpContextAccessor httpContextAccessor) : ICurrentUserService
{
    public Guid? UserId
    {
        get
        {
            var userIdClaim = httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);
            return Guid.TryParse(userIdClaim, out var userId) ? userId : null;
        }
    }

    public string? Username
    {
        get
        {
            return httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.Name);
        }
    }

    public Guid? TenantId
    {
        get
        {
            var tenantIdClaim = httpContextAccessor.HttpContext?.User?.FindFirstValue("tenant_id");
            return Guid.TryParse(tenantIdClaim, out var tenantId) ? tenantId : null;
        }
    }

    public bool IsAuthenticated 
        => httpContextAccessor.HttpContext?.User?.Identity?.IsAuthenticated ?? false;
}
