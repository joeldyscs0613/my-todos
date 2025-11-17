using MyTodos.BuildingBlocks.Application.Abstractions.Commands;
using MyTodos.Services.IdentityService.Application.Features.Authentication.DTOs;

namespace MyTodos.Services.IdentityService.Application.Features.Authentication.Commands.SignIn;

/// <summary>
/// Command to sign in a user with username/email and password.
/// </summary>
public sealed class SignInCommand : ResponseCommand<SignInResponseDto>
{
    public string UsernameOrEmail { get; init; } = string.Empty;
    public string Password { get; init; } = string.Empty;
}
