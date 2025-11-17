using MyTodos.BuildingBlocks.Application.Abstractions.Commands;
using MyTodos.Services.IdentityService.Application.Common.Authentication.DTOs;

namespace MyTodos.Services.IdentityService.Application.Common.Authentication.Commands.RegisterFromInvitation;

/// <summary>
/// Command to register a new user from an invitation token.
/// </summary>
public sealed class RegisterFromInvitationCommand : ResponseCommand<SignInResponseDto>
{
    public string InvitationToken { get; init; } = string.Empty;
    public string Username { get; init; } = string.Empty;
    public string Password { get; init; } = string.Empty;
    public string? FirstName { get; init; }
    public string? LastName { get; init; }
}
