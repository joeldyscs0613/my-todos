using MyTodos.BuildingBlocks.Application.Abstractions.Commands;

namespace MyTodos.Services.IdentityService.Application.Users.Commands.InviteUser;

/// <summary>
/// Command to invite a user to join a tenant or as a global role.
/// </summary>
public sealed class InviteUserCommand : CreateCommand<Guid>
{
    public string Email { get; init; } = string.Empty;
    public Guid RoleId { get; init; }
    public Guid? TenantId { get; init; }
}
