using System.Net;
using System.Net.Mail;
using TalentVerse.WebAPI.Interfaces;

namespace TalentVerse.WebAPI.Services;

public class EmailService : IEmailService
{
    private readonly IConfiguration _config;
    private readonly ILogger<EmailService> _logger;
    private static readonly char[] WhitespaceChars = [' ', '\t', '\r', '\n'];

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
            var smtpUser = _config["Email:SmtpUser"]?.Trim();
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
            else
            {
                fromEmail = fromEmail.Trim();
            }

            if (string.IsNullOrWhiteSpace(fromName))
            {
                fromName = "TalentVerse";
            }

            smtpPass = NormalizeSmtpPassword(smtpHost, smtpPass);

            using var client = new SmtpClient(smtpHost, smtpPort)
            {
                DeliveryMethod = SmtpDeliveryMethod.Network,
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential(smtpUser, smtpPass),
                EnableSsl = true,
                Timeout = 10000
            };

            using var message = new MailMessage
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
        catch (SmtpException ex) when (IsLikelyGmailAuthenticationError(ex))
        {
            _logger.LogError(
                ex,
                "Gmail SMTP authentication failed for {Email}. Configure Email:SmtpUser as the full Gmail address and Email:SmtpPassword as a valid Google App Password, not the normal account password.",
                toEmail);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send email to {Email}", toEmail);
            throw;
        }
    }

    private string NormalizeSmtpPassword(string? smtpHost, string smtpPassword)
    {
        if (!IsGmailHost(smtpHost) || !smtpPassword.Any(char.IsWhiteSpace))
            return smtpPassword;

        _logger.LogWarning(
            "Whitespace was removed from Email:SmtpPassword for Gmail SMTP. Google App Passwords should be configured as the 16-character password without spaces.");

        return string.Concat(smtpPassword.Where(c => !WhitespaceChars.Contains(c)));
    }

    private static bool IsGmailHost(string? smtpHost)
    {
        return string.Equals(smtpHost, "smtp.gmail.com", StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsLikelyGmailAuthenticationError(SmtpException ex)
    {
        return ex.Message.Contains("5.7.0", StringComparison.OrdinalIgnoreCase)
            || ex.Message.Contains("Authentication Required", StringComparison.OrdinalIgnoreCase);
    }

    public async Task SendProposalCreatedAsync(
        string recipientEmail, 
        string recipientName, 
        string proposerName, 
        string offeredSkill, 
        string requestedSkill, 
        string? message)
    {
        var subject = "New Skill Swap Proposal Received";
        
        var body = $@"Hello {recipientName},

You have received a new skill swap proposal from {proposerName}!

Proposal Details:
• {proposerName} offers to teach: {offeredSkill}
• {proposerName} wants to learn: {requestedSkill}

{(string.IsNullOrWhiteSpace(message) ? "" : $@"Message from {proposerName}:
""{message}""

")}To review and respond to this proposal, please visit your TalentVerse dashboard.

Best regards,
TalentVerse Team";

        await SendEmailAsync(recipientEmail, subject, body);
    }

    public async Task SendProposalAcceptedAsync(
        string proposerEmail, 
        string proposerName, 
        string recipientName, 
        string offeredSkill, 
        string requestedSkill)
    {
        var subject = "Your Skill Swap Proposal Has Been Accepted!";
        
        var body = $@"Hello {proposerName},

Great news! {recipientName} has accepted your skill swap proposal.

Swap Details:
• You will teach: {offeredSkill}
• You will learn: {requestedSkill}

Next Steps:
1. Contact {recipientName} to schedule your first session
2. Once you've completed the skill exchange, both parties should confirm completion

Best regards,
TalentVerse Team";

        await SendEmailAsync(proposerEmail, subject, body);
    }

    public async Task SendProposalDeclinedAsync(
        string proposerEmail, 
        string proposerName, 
        string recipientName, 
        string offeredSkill, 
        string requestedSkill)
    {
        var subject = "Skill Swap Proposal Update";
        
        var body = $@"Hello {proposerName},

{recipientName} has declined your skill swap proposal.

Proposal Details:
• You offered: {offeredSkill}
• You requested: {requestedSkill}

Don't worry! There are many other talented users on TalentVerse who might be interested in exchanging skills with you.

Best regards,
TalentVerse Team";

        await SendEmailAsync(proposerEmail, subject, body);
    }

    public async Task SendProposalCompletedAsync(
        string userEmail, 
        string userName, 
        string otherUserName, 
        string offeredSkill, 
        string requestedSkill)
    {
        var subject = "Skill Swap Completed Successfully!";
        
        var body = $@"Hello {userName},

Congratulations! Your skill swap with {otherUserName} has been marked as completed by both parties.

Completed Swap:
• You taught: {offeredSkill}
• You learned: {requestedSkill}

We hope this was a valuable learning experience! Consider leaving a review for {otherUserName} to help other users find great learning partners.

Keep exploring and learning!

Best regards,
TalentVerse Team";

        await SendEmailAsync(userEmail, subject, body);
    }

    public async Task SendProposalCancelledAsync(
        string recipientEmail, 
        string recipientName, 
        string proposerName, 
        string offeredSkill, 
        string requestedSkill)
    {
        var subject = "Skill Swap Proposal Cancelled";
        
        var body = $@"Hello {recipientName},

{proposerName} has cancelled their skill swap proposal.

Proposal Details:
• They offered: {offeredSkill}
• They wanted to learn: {requestedSkill}

This proposal is no longer active. You can continue browsing other opportunities on TalentVerse.

Best regards,
TalentVerse Team";

        await SendEmailAsync(recipientEmail, subject, body);
    }

    public async Task SendVerificationSubmittedAsync(string userEmail, string userName)
    {
        var subject = "Identity Verification Request Received";

        var body = $@"Hello {userName},

Thank you for submitting your identity verification request!

We have received your documents and they are now being reviewed by our team. This process typically takes 1-3 business days.

You will receive an email notification once your verification has been processed.

If you have any questions, please don't hesitate to contact our support team.

Best regards,
TalentVerse Team";

        await SendEmailAsync(userEmail, subject, body);
    }

    public async Task SendVerificationApprovedAsync(string userEmail, string userName)
    {
        var subject = "Your Identity Has Been Verified!";

        var body = $@"Hello {userName},

Congratulations! Your identity has been successfully verified.

You have been awarded the ""Verified"" badge and 25 credits as a reward for completing the verification process.

Your verified status will be displayed on your profile, helping other users trust and connect with you more easily.

Thank you for being a trusted member of the TalentVerse community!

Best regards,
TalentVerse Team";

        await SendEmailAsync(userEmail, subject, body);
    }

    public async Task SendVerificationRejectedAsync(string userEmail, string userName, string reason)
    {
        var subject = "Identity Verification Update";

        var body = $@"Hello {userName},

We have reviewed your identity verification request and unfortunately, we were unable to verify your identity at this time.

Reason: {reason}

You are welcome to submit a new verification request with updated documents. Please ensure:
• The document is clearly legible
• All information is visible
• The document is a valid government-issued ID

If you believe this was an error or have questions, please contact our support team.

Best regards,
TalentVerse Team";

        await SendEmailAsync(userEmail, subject, body);
    }
}
