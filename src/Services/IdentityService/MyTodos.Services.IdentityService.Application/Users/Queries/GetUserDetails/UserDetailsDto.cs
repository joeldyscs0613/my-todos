namespace MyTodos.Services.IdentityService.Application.Users.Queries.GetUserDetails;

/// <summary>
/// User details DTO.
/// </summary>
public sealed record UserDetailsDto
{
    public Guid Id { get; init; }
    public string Username { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string? FirstName { get; init; }
    public string? LastName { get; init; }
    public bool IsActive { get; init; }
    public DateTime? LastLoginAt { get; init; }
    public IReadOnlyList<UserRoleDto> Roles { get; init; } = new List<UserRoleDto>();
}

public sealed record UserRoleDto
{
    public Guid RoleId { get; init; }
    public string RoleName { get; init; } = string.Empty;
    public Guid? TenantId { get; init; }
    public string? TenantName { get; init; }
}
