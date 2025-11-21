using Microsoft.AspNetCore.Authorization;

namespace MyTodos.BuildingBlocks.Presentation.Authorization;

/// <summary>
/// Authorization requirement that allows certain paths to bypass authentication.
/// </summary>
public class ConditionalAuthenticationRequirement : IAuthorizationRequirement
{
    public HashSet<string> AnonymousPaths { get; } = new(StringComparer.OrdinalIgnoreCase)
    {
        "/health"
    };
}
