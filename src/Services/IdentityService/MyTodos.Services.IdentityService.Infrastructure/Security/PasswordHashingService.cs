using MyTodos.BuildingBlocks.Application.Contracts.Security;

namespace MyTodos.Services.IdentityService.Infrastructure.Security;

/// <summary>
/// Password hashing service using BCrypt.
/// </summary>
public sealed class PasswordHashingService : IPasswordHashingService
{
    private const int WorkFactor = 12; // BCrypt work factor (higher = more secure but slower)

    public string HashPassword(string password)
    {
        if (string.IsNullOrWhiteSpace(password))
            throw new ArgumentException("Password cannot be null or whitespace.", nameof(password));

        return BCrypt.Net.BCrypt.HashPassword(password, WorkFactor);
    }

    public bool VerifyPassword(string password, string passwordHash)
    {
        if (string.IsNullOrWhiteSpace(password))
            return false;

        if (string.IsNullOrWhiteSpace(passwordHash))
            return false;

        try
        {
            return BCrypt.Net.BCrypt.Verify(password, passwordHash);
        }
        catch
        {
            // Invalid hash format or other BCrypt errors
            return false;
        }
    }
}
