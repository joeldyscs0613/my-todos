namespace MyTodos.Services.NotificationService.Infrastructure.Email;

/// <summary>
/// Configuration settings for email service.
/// </summary>
public sealed class EmailSettings
{
    public const string SectionName = "Email";

    /// <summary>
    /// SMTP server host address.
    /// </summary>
    public string SmtpHost { get; init; } = string.Empty;

    /// <summary>
    /// SMTP server port.
    /// </summary>
    public int SmtpPort { get; init; } = 587;

    /// <summary>
    /// Whether to use SSL for SMTP connection.
    /// </summary>
    public bool EnableSsl { get; init; } = false;

    /// <summary>
    /// SMTP username (if authentication is required).
    /// </summary>
    public string? Username { get; init; }

    /// <summary>
    /// SMTP password (if authentication is required).
    /// </summary>
    public string? Password { get; init; }

    /// <summary>
    /// Email address to use as the sender.
    /// </summary>
    public string FromAddress { get; init; } = string.Empty;

    /// <summary>
    /// Display name for the sender.
    /// </summary>
    public string FromName { get; init; } = string.Empty;
}
