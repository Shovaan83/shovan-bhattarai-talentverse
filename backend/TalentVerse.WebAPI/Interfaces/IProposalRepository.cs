using TalentVerse.WebAPI.Data.Entities;
using TalentVerse.WebAPI.Data.Enums;
using TalentVerse.WebAPI.DTO.Proposals;

namespace TalentVerse.WebAPI.Interfaces
{
    public interface IProposalRepository
    {
        /// <summary>
        /// Create a new proposal
        /// </summary>
        Task<Proposal?> CreateAsync(Proposal proposal);

        /// <summary>
        /// Get a proposal by ID with all related data
        /// </summary>
        Task<ProposalDto?> GetByIdAsync(int proposalId);

        /// <summary>
        /// Get the raw proposal entity by ID (for updates)
        /// </summary>
        Task<Proposal?> GetEntityByIdAsync(int proposalId);

        /// <summary>
        /// Get paginated proposals for a user with filters
        /// </summary>
        Task<(List<ProposalListDto> Proposals, int TotalCount)> GetUserProposalsAsync(
            string userId, 
            ProposalFilterDto filter);

        /// <summary>
        /// Update proposal status
        /// </summary>
        Task<bool> UpdateStatusAsync(int proposalId, ProposalStatus newStatus);

        /// <summary>
        /// Update completion confirmation for a user
        /// </summary>
        Task<bool> UpdateCompletionConfirmationAsync(int proposalId, string oderId, bool isProposer);

        /// <summary>
        /// Check if there's an active proposal between two users for the same skills
        /// </summary>
        Task<bool> HasActiveProposalAsync(
            string proposerId, 
            string recipientId, 
            int proposerUserSkillId, 
            int recipientUserSkillId);

        /// <summary>
        /// Get the owner of a UserSkill
        /// </summary>
        Task<string?> GetUserSkillOwnerAsync(int userSkillId);

        /// <summary>
        /// Validate that the proposer skill is an Offer and recipient skill is an Offer (to be received)
        /// </summary>
        Task<(bool IsValid, string? ErrorMessage)> ValidateSkillsForProposalAsync(
            string proposerId,
            int proposerUserSkillId, 
            int recipientUserSkillId);
    }
}
