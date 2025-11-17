using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyTodos.BuildingBlocks.Presentation.Extensions;
using MyTodos.Services.IdentityService.Application.Features.Authentication.Commands.RegisterFromInvitation;
using MyTodos.Services.IdentityService.Application.Features.Authentication.Commands.SignIn;
using MyTodos.Services.IdentityService.Application.Features.Authentication.DTOs;

namespace MyTodos.Services.IdentityService.Api.Controllers;

/// <summary>
/// Handles authentication operations (sign-in, registration).
/// </summary>
[ApiController]
[Route("api/auth")]
public sealed class AuthController : ControllerBase
{
    private readonly ISender _sender;

    public AuthController(ISender sender)
    {
        _sender = sender;
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
        var result = await _sender.Send(command, ct);
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
        var result = await _sender.Send(command, ct);
        return result.ToActionResult();
    }
}
