namespace MyTodos.Services.NotificationService.Application.Common.Contracts;

/// <summary>
/// Service for sending emails.
/// </summary>
public interface IEmailService
{
    /// <summary>
    /// Sends an email with the specified subject and body.
    /// </summary>
    /// <param name="toEmail">Recipient's email address</param>
    /// <param name="subject">Email subject</param>
    /// <param name="body">Email body content</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>True if email was sent successfully, false otherwise</returns>
    Task<bool> SendEmailAsync(
        string toEmail,
        string subject,
        string body,
        CancellationToken ct = default);
}
