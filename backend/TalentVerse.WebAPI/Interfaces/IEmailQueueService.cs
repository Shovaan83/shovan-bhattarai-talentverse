using TalentVerse.WebAPI.Common;

namespace TalentVerse.WebAPI.Interfaces;

/// <summary>
/// Service for queuing emails to be sent in the background
/// </summary>
public interface IEmailQueueService
{
    /// <summary>
    /// Queue an email to be sent asynchronously
    /// </summary>
    /// <param name="toEmail">Recipient email address</param>
    /// <param name="subject">Email subject</param>
    /// <param name="body">Email body (plain text)</param>
    ValueTask QueueEmailAsync(string toEmail, string subject, string body);
}
