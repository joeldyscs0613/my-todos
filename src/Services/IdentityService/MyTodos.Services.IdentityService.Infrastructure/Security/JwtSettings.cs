namespace MyTodos.Services.IdentityService.Infrastructure.Security;

/// <summary>
/// JWT token configuration settings.
/// </summary>
public sealed class JwtSettings
{
    public const string SectionName = "Jwt";

    public string SecretKey { get; set; } = string.Empty;
    public string Issuer { get; set; } = string.Empty;
    public string Audience { get; set; } = string.Empty;
    public int ExpirationMinutes { get; set; } = 60; // Default: 1 hour
}
