using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;

namespace MyTodos.BuildingBlocks.Presentation.Authorization;

/// <summary>
/// Authorization handler that allows anonymous access to specific paths (like /health).
/// </summary>
public class ConditionalAuthenticationHandler : AuthorizationHandler<ConditionalAuthenticationRequirement>
{
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        ConditionalAuthenticationRequirement requirement)
    {
        // Check if endpoint allows anonymous (has AllowAnonymous metadata)
        if (context.Resource is HttpContext httpContext)
        {
            var endpoint = httpContext.GetEndpoint();
            var allowAnonymous = endpoint?.Metadata.GetMetadata<IAllowAnonymous>() != null;

            if (allowAnonymous)
            {
                // Allow anonymous access for endpoints marked with [AllowAnonymous]
                context.Succeed(requirement);
                return Task.CompletedTask;
            }

            var requestPath = httpContext.Request.Path.Value ?? string.Empty;

            // Check if the current path is in the anonymous paths list
            if (requirement.AnonymousPaths.Contains(requestPath))
            {
                // Allow anonymous access for this path
                context.Succeed(requirement);
                return Task.CompletedTask;
            }
        }

        // For all other paths, require authenticated user
        if (context.User.Identity?.IsAuthenticated == true)
        {
            context.Succeed(requirement);
        }

        return Task.CompletedTask;
    }
}
