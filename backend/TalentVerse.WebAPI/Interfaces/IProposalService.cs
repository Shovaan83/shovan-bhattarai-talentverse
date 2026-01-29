using TalentVerse.WebAPI.Common;
using TalentVerse.WebAPI.DTO.Proposals;

namespace TalentVerse.WebAPI.Interfaces
{
    public interface IProposalService
    {
        /// <summary>
        /// Create a new swap proposal
        /// </summary>
        Task<ServiceResponse<ProposalDto>> CreateProposalAsync(string userId, CreateProposalDto dto);

        /// <summary>
        /// Get a single proposal by ID (with authorization check)
        /// </summary>
        Task<ServiceResponse<ProposalDto>> GetProposalAsync(string userId, int proposalId);

        /// <summary>
        /// Get paginated proposals for the current user
        /// </summary>
        Task<ServiceResponse<ProposalListResponseDto>> GetUserProposalsAsync(string userId, ProposalFilterDto filter);

        /// <summary>
        /// Accept a proposal (recipient only, Pending → Accepted)
        /// </summary>
        Task<ServiceResponse<ProposalDto>> AcceptProposalAsync(string userId, int proposalId);

        /// <summary>
        /// Decline a proposal (recipient only, Pending → Rejected)
        /// </summary>
        Task<ServiceResponse<ProposalDto>> DeclineProposalAsync(string userId, int proposalId);

        /// <summary>
        /// Cancel a proposal (proposer only, Pending → Cancelled)
        /// </summary>
        Task<ServiceResponse<ProposalDto>> CancelProposalAsync(string userId, int proposalId);

        /// <summary>
        /// Confirm completion of a swap (Accepted → marks user's confirmation, both → Completed)
        /// </summary>
        Task<ServiceResponse<ProposalDto>> ConfirmCompletionAsync(string userId, int proposalId);
    }
}
