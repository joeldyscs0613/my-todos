using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyTodos.BuildingBlocks.Presentation.Authorization;
using MyTodos.BuildingBlocks.Presentation.Controllers;
using MyTodos.BuildingBlocks.Presentation.Extensions;
using MyTodos.Services.IdentityService.Application.Common.Authentication.Commands.Logout;
using MyTodos.Services.IdentityService.Application.Common.Authentication.Commands.RegisterFromInvitation;
using MyTodos.Services.IdentityService.Application.Common.Authentication.Commands.SignIn;
using MyTodos.Services.IdentityService.Application.Common.Authentication.DTOs;
using MyTodos.Services.IdentityService.Contracts;

namespace MyTodos.Services.IdentityService.Api.Controllers;

/// <summary>
/// Handles authentication operations (sign-in, registration).
/// </summary>
[ApiController]
[Route("api/auth")]
public sealed class AuthController : ApiControllerBase
{
    public AuthController()
    {
    }

    /// <summary>
    /// Sign in with username/email and password.
    /// </summary>
    [AllowAnonymous]
    [HttpPost("signin")]
    [ProducesResponseType(typeof(SignInResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> SignIn([FromBody] SignInCommand command, CancellationToken ct)
    {
        var result = await Sender.Send(command, ct);
        return result.ToActionResult();
    }

    /// <summary>
    /// Register a new user from an invitation token.
    /// </summary>
    [AllowAnonymous]
    [HttpPost("register")]
    [ProducesResponseType(typeof(SignInResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Register([FromBody] RegisterFromInvitationCommand command, CancellationToken ct)
    {
        var result = await Sender.Send(command, ct);

        return result.ToActionResult();
    }

    /// <summary>
    /// Log out the current user.
    /// In a JWT-based system, the client should discard the token after calling this endpoint.
    /// This endpoint is primarily used for audit logging.
    /// </summary>
    [HttpPost("logout")]
    [HasPermission(Permissions.Auth.Logout)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Logout(CancellationToken ct)
    {
        var command = new LogoutCommand();
        var result = await Sender.Send(command, ct);

        return result.ToActionResult();
    }
}
