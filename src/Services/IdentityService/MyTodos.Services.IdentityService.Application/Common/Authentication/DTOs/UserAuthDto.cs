namespace MyTodos.Services.IdentityService.Application.Common.Authentication.DTOs;

/// <summary>
/// User authentication data transfer object.
/// </summary>
public sealed record UserAuthDto
{
    public Guid UserId { get; init; }
    public string Username { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string? FirstName { get; init; }
    public string? LastName { get; init; }
    public Guid? TenantId { get; init; }
    public IReadOnlyList<string> Roles { get; init; } = new List<string>();
}
