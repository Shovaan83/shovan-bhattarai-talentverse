namespace TalentVerse.WebAPI.Common;

/// <summary>
/// Represents an email to be sent via the background queue
/// </summary>
public class EmailQueueItem
{
    public string ToEmail { get; set; } = string.Empty;
    public string Subject { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public DateTime QueuedAt { get; set; } = DateTime.UtcNow;
}
