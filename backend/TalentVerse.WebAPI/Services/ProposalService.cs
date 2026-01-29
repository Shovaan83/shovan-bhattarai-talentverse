using Microsoft.AspNetCore.Identity;
using TalentVerse.WebAPI.Common;
using TalentVerse.WebAPI.Data.Entities;
using TalentVerse.WebAPI.Data.Enums;
using TalentVerse.WebAPI.DTO.Proposals;
using TalentVerse.WebAPI.Interfaces;

namespace TalentVerse.WebAPI.Services
{
    public class ProposalService : IProposalService
    {
        private readonly IProposalRepository _proposalRepo;
        private readonly UserManager<AppUser> _userManager;
        private readonly ILogger<ProposalService> _logger;

        public ProposalService(
            IProposalRepository proposalRepo, 
            UserManager<AppUser> userManager,
            ILogger<ProposalService> logger)
        {
            _proposalRepo = proposalRepo;
            _userManager = userManager;
            _logger = logger;
        }

        public async Task<ServiceResponse<ProposalDto>> CreateProposalAsync(string userId, CreateProposalDto dto)
        {
            try
            {
                _logger.LogInformation(
                    "CreateProposalAsync called - UserId: {UserId}, ProposerSkillId: {ProposerSkillId}, RecipientSkillId: {RecipientSkillId}",
                    userId, dto?.ProposerUserSkillId, dto?.RecipientUserSkillId);

                // 1. Guard clauses
                if (string.IsNullOrWhiteSpace(userId))
                    return ServiceResponse<ProposalDto>.FailureResponse("User ID is required.");

                if (dto == null)
                    return ServiceResponse<ProposalDto>.FailureResponse("Proposal data is required.");

                // 2. Check profile completeness (soft-lock)
                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                    return ServiceResponse<ProposalDto>.FailureResponse("User not found.");

                if (!user.IsProfileComplete)
                {
                    _logger.LogWarning("User {UserId} attempted to create proposal without completing profile", userId);
                    return ServiceResponse<ProposalDto>.FailureResponse(
                        AppConstant.ErrorMessages.ProfileNotComplete);
                }

                // 3. Validate skills (ownership, type, not self-swap)
                _logger.LogInformation("Calling ValidateSkillsForProposalAsync...");
                var validation = await _proposalRepo.ValidateSkillsForProposalAsync(
                    userId,
                    dto.ProposerUserSkillId,
                    dto.RecipientUserSkillId);

                _logger.LogInformation("Validation result - IsValid: {IsValid}, ErrorMessage: {ErrorMessage}", 
                    validation.IsValid, validation.ErrorMessage);

                if (!validation.IsValid)
                    return ServiceResponse<ProposalDto>.FailureResponse(validation.ErrorMessage!);

                // 4. Get recipient ID from their skill
                _logger.LogInformation("Getting recipient ID for skill {SkillId}...", dto.RecipientUserSkillId);
                var recipientId = await _proposalRepo.GetUserSkillOwnerAsync(dto.RecipientUserSkillId);
                _logger.LogInformation("Recipient ID: {RecipientId}", recipientId);
                if (string.IsNullOrEmpty(recipientId))
                    return ServiceResponse<ProposalDto>.FailureResponse("Could not determine the skill owner.");

                // 5. Check for duplicate active proposals
                _logger.LogInformation("Checking for duplicate active proposals...");
                var hasActive = await _proposalRepo.HasActiveProposalAsync(
                    userId,
                    recipientId,
                    dto.ProposerUserSkillId,
                    dto.RecipientUserSkillId);
                _logger.LogInformation("Has active proposal: {HasActive}", hasActive);

                if (hasActive)
                    return ServiceResponse<ProposalDto>.FailureResponse(
                        "You already have an active proposal for these skills. Please wait for a response or cancel the existing proposal.");

                // 6. Create the proposal
                _logger.LogInformation("Creating proposal...");
                var proposal = new Proposal
                {
                    ProposerId = userId,
                    RecipientId = recipientId,
                    ProposerUserSkillId = dto.ProposerUserSkillId,
                    RecipientUserSkillId = dto.RecipientUserSkillId,
                    Status = ProposalStatus.Pending,
                    ProposerConfirmed = false,
                    RecipientConfirmed = false,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                var created = await _proposalRepo.CreateAsync(proposal);
                _logger.LogInformation("Created proposal ID: {ProposalId}", created?.ProposalId);

                if (created == null)
                    return ServiceResponse<ProposalDto>.FailureResponse("Failed to create proposal.");

                // 7. Fetch full details for response
                _logger.LogInformation("Fetching proposal details...");
                var proposalDto = await _proposalRepo.GetByIdAsync(created.ProposalId);
                _logger.LogInformation("Proposal DTO retrieved: {Success}", proposalDto != null);

                if (proposalDto == null)
                    return ServiceResponse<ProposalDto>.FailureResponse("Proposal created but failed to retrieve details.");

                // Add action flags for the proposer
                proposalDto.CanCancel = true;
                proposalDto.CanAccept = false;
                proposalDto.CanDecline = false;
                proposalDto.CanConfirmCompletion = false;

                _logger.LogInformation(
                    "Proposal {ProposalId} created by {ProposerId} for {RecipientId}",
                    created.ProposalId, userId, recipientId);

                return ServiceResponse<ProposalDto>.SuccessResponse(
                    proposalDto,
                    AppConstant.SuccessMessages.ProposalSent);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating proposal for user {UserId}", userId);
                return ServiceResponse<ProposalDto>.FailureResponse(AppConstant.ErrorMessages.GenericError);
            }
        }

        public async Task<ServiceResponse<ProposalDto>> GetProposalAsync(string userId, int proposalId)
        {
            try
            {
                // 1. Guard clauses
                if (string.IsNullOrWhiteSpace(userId))
                    return ServiceResponse<ProposalDto>.FailureResponse("User ID is required.");

                if (proposalId <= 0)
                    return ServiceResponse<ProposalDto>.FailureResponse("Invalid proposal ID.");

                // 2. Get proposal
                var proposal = await _proposalRepo.GetByIdAsync(proposalId);

                if (proposal == null)
                    return ServiceResponse<ProposalDto>.FailureResponse(AppConstant.ErrorMessages.ProposalNotFound);

                // 3. Authorization check - only proposer or recipient can view
                if (proposal.ProposerId != userId && proposal.RecipientId != userId)
                    return ServiceResponse<ProposalDto>.FailureResponse("You are not authorized to view this proposal.");

                // 4. Set action flags based on user role and status
                SetActionFlags(proposal, userId);

                return ServiceResponse<ProposalDto>.SuccessResponse(proposal);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting proposal {ProposalId} for user {UserId}", proposalId, userId);
                return ServiceResponse<ProposalDto>.FailureResponse(AppConstant.ErrorMessages.GenericError);
            }
        }

        public async Task<ServiceResponse<ProposalListResponseDto>> GetUserProposalsAsync(string userId, ProposalFilterDto filter)
        {
            try
            {
                // 1. Guard clauses
                if (string.IsNullOrWhiteSpace(userId))
                    return ServiceResponse<ProposalListResponseDto>.FailureResponse("User ID is required.");

                // 2. Validate and sanitize filter
                filter ??= new ProposalFilterDto();
                filter.Page = Math.Max(1, filter.Page);
                filter.PageSize = Math.Clamp(filter.PageSize, 1, 50);

                // 3. Get proposals
                var (proposals, totalCount) = await _proposalRepo.GetUserProposalsAsync(userId, filter);

                var response = new ProposalListResponseDto
                {
                    Proposals = proposals,
                    TotalCount = totalCount,
                    Page = filter.Page,
                    PageSize = filter.PageSize
                };

                return ServiceResponse<ProposalListResponseDto>.SuccessResponse(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting proposals for user {UserId}", userId);
                return ServiceResponse<ProposalListResponseDto>.FailureResponse(AppConstant.ErrorMessages.GenericError);
            }
        }

        public async Task<ServiceResponse<ProposalDto>> AcceptProposalAsync(string userId, int proposalId)
        {
            try
            {
                // 1. Guard clauses
                if (string.IsNullOrWhiteSpace(userId))
                    return ServiceResponse<ProposalDto>.FailureResponse("User ID is required.");

                // 2. Get proposal entity
                var proposal = await _proposalRepo.GetEntityByIdAsync(proposalId);

                if (proposal == null)
                    return ServiceResponse<ProposalDto>.FailureResponse(AppConstant.ErrorMessages.ProposalNotFound);

                // 3. Authorization - only recipient can accept
                if (proposal.RecipientId != userId)
                    return ServiceResponse<ProposalDto>.FailureResponse("Only the recipient can accept a proposal.");

                // 4. State machine validation - only Pending can be Accepted
                if (proposal.Status != ProposalStatus.Pending)
                    return ServiceResponse<ProposalDto>.FailureResponse(AppConstant.ErrorMessages.InvalidStateTransition);

                // 5. Update status
                var success = await _proposalRepo.UpdateStatusAsync(proposalId, ProposalStatus.Accepted);

                if (!success)
                    return ServiceResponse<ProposalDto>.FailureResponse("Failed to accept proposal.");

                // 6. Return updated proposal
                var updatedProposal = await _proposalRepo.GetByIdAsync(proposalId);
                if (updatedProposal != null)
                    SetActionFlags(updatedProposal, userId);

                _logger.LogInformation(
                    "Proposal {ProposalId} accepted by {UserId}",
                    proposalId, userId);

                return ServiceResponse<ProposalDto>.SuccessResponse(
                    updatedProposal!,
                    AppConstant.SuccessMessages.ProposalAccepted);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error accepting proposal {ProposalId} for user {UserId}", proposalId, userId);
                return ServiceResponse<ProposalDto>.FailureResponse(AppConstant.ErrorMessages.GenericError);
            }
        }

        public async Task<ServiceResponse<ProposalDto>> DeclineProposalAsync(string userId, int proposalId)
        {
            try
            {
                // 1. Guard clauses
                if (string.IsNullOrWhiteSpace(userId))
                    return ServiceResponse<ProposalDto>.FailureResponse("User ID is required.");

                // 2. Get proposal entity
                var proposal = await _proposalRepo.GetEntityByIdAsync(proposalId);

                if (proposal == null)
                    return ServiceResponse<ProposalDto>.FailureResponse(AppConstant.ErrorMessages.ProposalNotFound);

                // 3. Authorization - only recipient can decline
                if (proposal.RecipientId != userId)
                    return ServiceResponse<ProposalDto>.FailureResponse("Only the recipient can decline a proposal.");

                // 4. State machine validation - only Pending can be Declined
                if (proposal.Status != ProposalStatus.Pending)
                    return ServiceResponse<ProposalDto>.FailureResponse(AppConstant.ErrorMessages.InvalidStateTransition);

                // 5. Update status
                var success = await _proposalRepo.UpdateStatusAsync(proposalId, ProposalStatus.Rejected);

                if (!success)
                    return ServiceResponse<ProposalDto>.FailureResponse("Failed to decline proposal.");

                // 6. Return updated proposal
                var updatedProposal = await _proposalRepo.GetByIdAsync(proposalId);
                if (updatedProposal != null)
                    SetActionFlags(updatedProposal, userId);

                _logger.LogInformation(
                    "Proposal {ProposalId} declined by {UserId}",
                    proposalId, userId);

                return ServiceResponse<ProposalDto>.SuccessResponse(
                    updatedProposal!,
                    AppConstant.SuccessMessages.ProposalDeclined);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error declining proposal {ProposalId} for user {UserId}", proposalId, userId);
                return ServiceResponse<ProposalDto>.FailureResponse(AppConstant.ErrorMessages.GenericError);
            }
        }

        public async Task<ServiceResponse<ProposalDto>> CancelProposalAsync(string userId, int proposalId)
        {
            try
            {
                // 1. Guard clauses
                if (string.IsNullOrWhiteSpace(userId))
                    return ServiceResponse<ProposalDto>.FailureResponse("User ID is required.");

                // 2. Get proposal entity
                var proposal = await _proposalRepo.GetEntityByIdAsync(proposalId);

                if (proposal == null)
                    return ServiceResponse<ProposalDto>.FailureResponse(AppConstant.ErrorMessages.ProposalNotFound);

                // 3. Authorization - only proposer can cancel
                if (proposal.ProposerId != userId)
                    return ServiceResponse<ProposalDto>.FailureResponse("Only the proposer can cancel a proposal.");

                // 4. State machine validation - only Pending can be Cancelled
                if (proposal.Status != ProposalStatus.Pending)
                    return ServiceResponse<ProposalDto>.FailureResponse(AppConstant.ErrorMessages.InvalidStateTransition);

                // 5. Update status
                var success = await _proposalRepo.UpdateStatusAsync(proposalId, ProposalStatus.Cancelled);

                if (!success)
                    return ServiceResponse<ProposalDto>.FailureResponse("Failed to cancel proposal.");

                // 6. Return updated proposal
                var updatedProposal = await _proposalRepo.GetByIdAsync(proposalId);
                if (updatedProposal != null)
                    SetActionFlags(updatedProposal, userId);

                _logger.LogInformation(
                    "Proposal {ProposalId} cancelled by {UserId}",
                    proposalId, userId);

                return ServiceResponse<ProposalDto>.SuccessResponse(
                    updatedProposal!,
                    "Proposal cancelled successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cancelling proposal {ProposalId} for user {UserId}", proposalId, userId);
                return ServiceResponse<ProposalDto>.FailureResponse(AppConstant.ErrorMessages.GenericError);
            }
        }

        public async Task<ServiceResponse<ProposalDto>> ConfirmCompletionAsync(string userId, int proposalId)
        {
            try
            {
                // 1. Guard clauses
                if (string.IsNullOrWhiteSpace(userId))
                    return ServiceResponse<ProposalDto>.FailureResponse("User ID is required.");

                // 2. Get proposal entity
                var proposal = await _proposalRepo.GetEntityByIdAsync(proposalId);

                if (proposal == null)
                    return ServiceResponse<ProposalDto>.FailureResponse(AppConstant.ErrorMessages.ProposalNotFound);

                // 3. Authorization - only proposer or recipient can confirm
                var isProposer = proposal.ProposerId == userId;
                var isRecipient = proposal.RecipientId == userId;

                if (!isProposer && !isRecipient)
                    return ServiceResponse<ProposalDto>.FailureResponse("You are not authorized to confirm this proposal.");

                // 4. State machine validation - only Accepted can be confirmed
                if (proposal.Status != ProposalStatus.Accepted)
                    return ServiceResponse<ProposalDto>.FailureResponse(
                        "Only accepted proposals can be marked as completed.");

                // 5. Check if user already confirmed
                if (isProposer && proposal.ProposerConfirmed)
                    return ServiceResponse<ProposalDto>.FailureResponse("You have already confirmed completion.");

                if (isRecipient && proposal.RecipientConfirmed)
                    return ServiceResponse<ProposalDto>.FailureResponse("You have already confirmed completion.");

                // 6. Update confirmation (this will also update status to Completed if both confirmed)
                var success = await _proposalRepo.UpdateCompletionConfirmationAsync(proposalId, userId, isProposer);

                if (!success)
                    return ServiceResponse<ProposalDto>.FailureResponse("Failed to confirm completion.");

                // 7. Return updated proposal
                var updatedProposal = await _proposalRepo.GetByIdAsync(proposalId);
                if (updatedProposal != null)
                    SetActionFlags(updatedProposal, userId);

                var message = updatedProposal?.Status == "Completed"
                    ? "Swap completed successfully! Both parties have confirmed."
                    : "Your completion has been confirmed. Waiting for the other party to confirm.";

                _logger.LogInformation(
                    "Proposal {ProposalId} completion confirmed by {UserId}. Status: {Status}",
                    proposalId, userId, updatedProposal?.Status);

                return ServiceResponse<ProposalDto>.SuccessResponse(updatedProposal!, message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error confirming completion for proposal {ProposalId} by user {UserId}", proposalId, userId);
                return ServiceResponse<ProposalDto>.FailureResponse(AppConstant.ErrorMessages.GenericError);
            }
        }

        /// <summary>
        /// Sets the action flags on a ProposalDto based on user role and proposal status
        /// </summary>
        private static void SetActionFlags(ProposalDto proposal, string userId)
        {
            var isProposer = proposal.ProposerId == userId;
            var isRecipient = proposal.RecipientId == userId;

            // Reset all flags
            proposal.CanAccept = false;
            proposal.CanDecline = false;
            proposal.CanCancel = false;
            proposal.CanConfirmCompletion = false;

            switch (proposal.Status)
            {
                case "Pending":
                    if (isRecipient)
                    {
                        proposal.CanAccept = true;
                        proposal.CanDecline = true;
                    }
                    if (isProposer)
                    {
                        proposal.CanCancel = true;
                    }
                    break;

                case "Accepted":
                    // Can confirm if not already confirmed
                    if (isProposer && !proposal.ProposerConfirmed)
                        proposal.CanConfirmCompletion = true;
                    if (isRecipient && !proposal.RecipientConfirmed)
                        proposal.CanConfirmCompletion = true;
                    break;

                // Rejected, Completed, Cancelled - no actions available
            }
        }
    }
}
