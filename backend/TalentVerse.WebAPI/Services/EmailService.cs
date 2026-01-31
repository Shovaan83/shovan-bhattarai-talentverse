using System.Net;
using System.Net.Mail;
using TalentVerse.WebAPI.Interfaces;

namespace TalentVerse.WebAPI.Services;

public class EmailService : IEmailService
{
    private readonly IConfiguration _config;
    private readonly ILogger<EmailService> _logger;

    public EmailService(IConfiguration config, ILogger<EmailService> logger)
    {
        _config = config;
        _logger = logger;
    }

    public async Task SendEmailAsync(string toEmail, string subject, string body)
    {
        // Guard clause: validate email
        if (string.IsNullOrWhiteSpace(toEmail))
        {
            _logger.LogError("{Method} called with null or empty email", nameof(SendEmailAsync));
            throw new ArgumentException("Email address is required.", nameof(toEmail));
        }

        try
        {
            // Read configuration with fallback defaults
            var smtpHost = _config["Email:SmtpHost"];
            var smtpPortStr = _config["Email:SmtpPort"];
            var smtpUser = _config["Email:SmtpUser"];
            var smtpPass = _config["Email:SmtpPassword"];
            var fromEmail = _config["Email:FromEmail"];
            var fromName = _config["Email:FromName"];

            // Development mode: log email instead of sending if credentials not configured
            if (string.IsNullOrWhiteSpace(smtpUser) || string.IsNullOrWhiteSpace(smtpPass))
            {
                _logger.LogWarning(
                    "Email configuration incomplete. Email would be sent to {Email}:\nSubject: {Subject}\n\n{Body}",
                    toEmail, subject, body);
                return; // Don't throw - allow development without SMTP
            }

            // Parse port with validation
            if (!int.TryParse(smtpPortStr, out var smtpPort))
            {
                smtpPort = 587; // Default SMTP port for TLS
                _logger.LogWarning("Invalid or missing Email:SmtpPort configuration. Using default: {Port}", smtpPort);
            }

            // Validate required settings
            if (string.IsNullOrWhiteSpace(smtpHost))
            {
                smtpHost = "smtp.gmail.com";
                _logger.LogWarning("Missing Email:SmtpHost configuration. Using default: {Host}", smtpHost);
            }

            if (string.IsNullOrWhiteSpace(fromEmail))
            {
                fromEmail = smtpUser;
            }

            if (string.IsNullOrWhiteSpace(fromName))
            {
                fromName = "TalentVerse";
            }

            using var client = new SmtpClient(smtpHost, smtpPort)
            {
                Credentials = new NetworkCredential(smtpUser, smtpPass),
                EnableSsl = true
            };

            var message = new MailMessage
            {
                From = new MailAddress(fromEmail, fromName),
                Subject = subject,
                Body = body,
                IsBodyHtml = false
            };
            message.To.Add(toEmail);

            await client.SendMailAsync(message);
            _logger.LogInformation("Email sent successfully to {Email}", toEmail);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send email to {Email}", toEmail);
            // Don't rethrow in development - allow app to continue even if email fails
            // In production, you may want to rethrow or handle differently
        }
    }
}