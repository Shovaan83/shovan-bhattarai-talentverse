namespace TalentVerse.WebAPI.Interfaces
{
    public interface IEmailService
    {
        Task SendEmailAsync(string toEmail, string subject, string body);

        // Proposal notification methods
        Task SendProposalCreatedAsync(string recipientEmail, string recipientName, string proposerName, string offeredSkill, string requestedSkill, string? message);
        Task SendProposalAcceptedAsync(string proposerEmail, string proposerName, string recipientName, string offeredSkill, string requestedSkill);
        Task SendProposalDeclinedAsync(string proposerEmail, string proposerName, string recipientName, string offeredSkill, string requestedSkill);
        Task SendProposalCompletedAsync(string userEmail, string userName, string otherUserName, string offeredSkill, string requestedSkill);
        Task SendProposalCancelledAsync(string recipientEmail, string recipientName, string proposerName, string offeredSkill, string requestedSkill);

        // Verification notification methods
        Task SendVerificationSubmittedAsync(string userEmail, string userName);
        Task SendVerificationApprovedAsync(string userEmail, string userName);
        Task SendVerificationRejectedAsync(string userEmail, string userName, string reason);
    }
}
