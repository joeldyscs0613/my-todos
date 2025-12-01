using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MyTodos.Services.NotificationService.Application.Common.Contracts;

namespace MyTodos.Services.NotificationService.Infrastructure.Email;

/// <summary>
/// SMTP-based email service implementation.
/// </summary>
public sealed class SmtpEmailService : IEmailService
{
    private readonly EmailSettings _settings;
    private readonly ILogger<SmtpEmailService> _logger;

    public SmtpEmailService(
        IOptions<EmailSettings> settings,
        ILogger<SmtpEmailService> logger)
    {
        _settings = settings.Value;
        _logger = logger;
    }

    public async Task<bool> SendEmailAsync(
        string toEmail,
        string subject,
        string body,
        CancellationToken ct = default)
    {
        try
        {
            using var message = new MailMessage
            {
                From = new MailAddress(_settings.FromAddress, _settings.FromName),
                Subject = subject,
                Body = body,
                IsBodyHtml = false
            };

            message.To.Add(toEmail);

            using var smtpClient = new SmtpClient(_settings.SmtpHost, _settings.SmtpPort)
            {
                EnableSsl = _settings.EnableSsl
            };

            // Add credentials if provided
            if (!string.IsNullOrWhiteSpace(_settings.Username) &&
                !string.IsNullOrWhiteSpace(_settings.Password))
            {
                smtpClient.Credentials = new NetworkCredential(
                    _settings.Username,
                    _settings.Password);
            }

            await smtpClient.SendMailAsync(message, ct);

            _logger.LogInformation(
                "Email sent successfully - To: {Email}, Subject: {Subject}",
                toEmail,
                subject);

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Failed to send email - To: {Email}, Subject: {Subject}",
                toEmail,
                subject);

            return false;
        }
    }
}
