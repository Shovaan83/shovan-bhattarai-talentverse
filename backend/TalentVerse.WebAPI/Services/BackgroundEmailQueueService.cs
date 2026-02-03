using System.Threading.Channels;
using TalentVerse.WebAPI.Common;
using TalentVerse.WebAPI.Interfaces;

namespace TalentVerse.WebAPI.Services;

/// <summary>
/// Background service that processes queued emails
/// Uses Channel<T> for thread-safe, high-performance queuing
/// </summary>
public class BackgroundEmailQueueService : BackgroundService, IEmailQueueService
{
    private readonly Channel<EmailQueueItem> _emailQueue;
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<BackgroundEmailQueueService> _logger;

    public BackgroundEmailQueueService(
        IServiceProvider serviceProvider,
        ILogger<BackgroundEmailQueueService> logger)
    {
        // Unbounded channel - can hold unlimited emails (consider bounded for production)
        _emailQueue = Channel.CreateUnbounded<EmailQueueItem>(new UnboundedChannelOptions
        {
            SingleReader = true // Only one background worker reads
        });
        
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    /// <summary>
    /// Queue an email to be sent in the background
    /// </summary>
    public async ValueTask QueueEmailAsync(string toEmail, string subject, string body)
    {
        var emailItem = new EmailQueueItem
        {
            ToEmail = toEmail,
            Subject = subject,
            Body = body,
            QueuedAt = DateTime.UtcNow
        };

        await _emailQueue.Writer.WriteAsync(emailItem);
        
        _logger.LogInformation(
            "Email queued for {Email} with subject: {Subject}",
            toEmail,
            subject);
    }

    /// <summary>
    /// Background task that continuously processes the email queue
    /// </summary>
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Background Email Queue Service started");

        await foreach (var emailItem in _emailQueue.Reader.ReadAllAsync(stoppingToken))
        {
            try
            {
                // Create a new scope for each email to get scoped services
                using var scope = _serviceProvider.CreateScope();
                var emailService = scope.ServiceProvider.GetRequiredService<IEmailService>();

                await emailService.SendEmailAsync(
                    emailItem.ToEmail,
                    emailItem.Subject,
                    emailItem.Body);

                _logger.LogInformation(
                    "Email sent successfully to {Email} (queued at {QueuedAt})",
                    emailItem.ToEmail,
                    emailItem.QueuedAt);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Failed to send email to {Email} with subject: {Subject}",
                    emailItem.ToEmail,
                    emailItem.Subject);
                
                // ⚠️ Email is dropped on failure
                // TODO: Consider implementing retry logic or dead-letter queue
            }
        }

        _logger.LogInformation("Background Email Queue Service stopped");
    }
}
