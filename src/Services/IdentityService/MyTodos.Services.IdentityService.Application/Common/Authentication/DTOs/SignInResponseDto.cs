namespace MyTodos.Services.IdentityService.Application.Features.Authentication.DTOs;

/// <summary>
/// Sign-in response containing JWT token and user information.
/// </summary>
public sealed record SignInResponseDto
{
    public string Token { get; init; } = string.Empty;
    public UserAuthDto User { get; init; } = null!;
    public DateTime ExpiresAt { get; init; }
}
