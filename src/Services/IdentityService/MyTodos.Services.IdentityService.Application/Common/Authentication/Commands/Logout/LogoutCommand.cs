using Microsoft.Extensions.Logging;
using MyTodos.BuildingBlocks.Application.Abstractions.Commands;
using MyTodos.BuildingBlocks.Application.Contracts.Security;
using MyTodos.SharedKernel.Helpers;

namespace MyTodos.Services.IdentityService.Application.Common.Authentication.Commands.Logout;

/// <summary>
/// Command to log out the current user.
/// In a JWT-based system, logout is primarily handled client-side by discarding the token.
/// This command is used for audit logging purposes.
/// </summary>
public sealed class LogoutCommand : Command
{
    // No properties needed - we get the user from the current user service
}

/// <summary>
/// Handler for logging out a user.
/// </summary>
public sealed class LogoutCommandHandler : CommandHandler<LogoutCommand>
{
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<LogoutCommandHandler> _logger;

    public LogoutCommandHandler(
        ICurrentUserService currentUserService,
        ILogger<LogoutCommandHandler> logger)
    {
        _currentUserService = currentUserService;
        _logger = logger;
    }

    public override Task<Result> Handle(LogoutCommand request, CancellationToken ct)
    {
        // Ensure user is authenticated
        if (!_currentUserService.UserId.HasValue)
        {
            return Task.FromResult(Unauthorized("User must be authenticated to logout"));
        }

        // Log the logout event for audit purposes
        _logger.LogInformation("User logged out: {UserId}", _currentUserService.UserId.Value);

        // In a JWT-based system, the actual logout happens client-side
        // The client should discard the token
        // If you implement token blacklisting or refresh tokens, you would revoke them here

        return Task.FromResult(Result.Success());
    }
}
