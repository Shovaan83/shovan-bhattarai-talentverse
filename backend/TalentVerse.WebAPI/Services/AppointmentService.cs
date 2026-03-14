using Microsoft.AspNetCore.Identity;
using TalentVerse.WebAPI.Common;
using TalentVerse.WebAPI.Data.Entities;
using TalentVerse.WebAPI.Data.Enums;
using TalentVerse.WebAPI.DTO.Appointments;
using TalentVerse.WebAPI.Interfaces;

namespace TalentVerse.WebAPI.Services
{
    public class AppointmentService : IAppointmentService
    {
        private static readonly int[] AllowedDurations = { 30, 60, 90, 120 };

        private readonly IAppointmentRepository _appointmentRepo;
        private readonly IProposalRepository _proposalRepo;
        private readonly IGoogleCalendarService _calendarService;
        private readonly UserManager<AppUser> _userManager;
        private readonly ILogger<AppointmentService> _logger;

        public AppointmentService(
            IAppointmentRepository appointmentRepo,
            IProposalRepository proposalRepo,
            IGoogleCalendarService calendarService,
            UserManager<AppUser> userManager,
            ILogger<AppointmentService> logger)
        {
            _appointmentRepo = appointmentRepo;
            _proposalRepo = proposalRepo;
            _calendarService = calendarService;
            _userManager = userManager;
            _logger = logger;
        }

        public async Task<ServiceResponse<AppointmentDto>> ScheduleAppointmentAsync(string userId, CreateAppointmentDto dto)
        {
            try
            {
                // 1. Guard clauses
                if (string.IsNullOrWhiteSpace(userId))
                    return ServiceResponse<AppointmentDto>.FailureResponse(AppConstant.ErrorMessages.UserIdRequired);

                // 2. Input sanitisation
                var description = dto.Description?.Trim();

                // 3. Fail-fast validation (cheap checks)
                if (dto.MeetingTime.ToUniversalTime() <= DateTime.UtcNow.AddMinutes(15))
                    return ServiceResponse<AppointmentDto>.FailureResponse(AppConstant.ErrorMessages.InvalidMeetingTime);

                if (!AllowedDurations.Contains(dto.Duration))
                    return ServiceResponse<AppointmentDto>.FailureResponse(AppConstant.ErrorMessages.InvalidDuration);

                // 4. Business rule validation (DB queries)
                var proposal = await _proposalRepo.GetEntityByIdAsync(dto.ProposalId);
                if (proposal == null)
                    return ServiceResponse<AppointmentDto>.FailureResponse(AppConstant.ErrorMessages.ProposalNotFound);

                var isProposer = proposal.ProposerId == userId;
                var isRecipient = proposal.RecipientId == userId;
                if (!isProposer && !isRecipient)
                    return ServiceResponse<AppointmentDto>.FailureResponse(AppConstant.ErrorMessages.UnauthorizedAppointmentAction);

                if (proposal.Status != ProposalStatus.Accepted)
                    return ServiceResponse<AppointmentDto>.FailureResponse(AppConstant.ErrorMessages.ProposalNotAccepted);

                // 5. Check Google Calendar connection
                var calendarStatus = await _calendarService.GetStatusAsync(userId);
                if (!calendarStatus.Success || calendarStatus.Data == null || !calendarStatus.Data.IsConnected)
                    return ServiceResponse<AppointmentDto>.FailureResponse(
                        calendarStatus.Data?.IsRevoked == true
                            ? AppConstant.ErrorMessages.GoogleCalendarRevoked
                            : AppConstant.ErrorMessages.GoogleCalendarNotConnected);

                // 6. Resolve the other participant's email for the invite
                var otherUserId = isProposer ? proposal.RecipientId : proposal.ProposerId;
                var otherUser = await _userManager.FindByIdAsync(otherUserId);
                var attendeeEmail = otherUser?.Email ?? string.Empty;

                // Resolve proposer/recipient skill names for the event title
                var proposerSkillName = proposal.ProposerUserSkill?.Skill?.SkillName ?? "Skill";
                var recipientSkillName = proposal.RecipientUserSkill?.Skill?.SkillName ?? "Skill";
                var eventTitle = $"TalentVerse: {proposerSkillName} ↔ {recipientSkillName}";

                // 7. Create Google Calendar event
                var meetingTimeUtc = DateTime.SpecifyKind(dto.MeetingTime.ToUniversalTime(), DateTimeKind.Utc);
                var calResult = await _calendarService.CreateEventAsync(
                    userId, attendeeEmail, eventTitle, description, meetingTimeUtc, dto.Duration);

                if (!calResult.Success)
                    return ServiceResponse<AppointmentDto>.FailureResponse(calResult.Message);

                // 8. Persist the appointment
                var appointment = new Appointment
                {
                    ProposalId = dto.ProposalId,
                    CreatedByUserId = userId,
                    MeetingTime = meetingTimeUtc,
                    Duration = dto.Duration,
                    Description = description,
                    MeetingLink = calResult.Data.MeetLink,
                    Status = AppointmentStatus.Scheduled,
                    GoogleCalendarEventId = calResult.Data.EventId,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                var created = await _appointmentRepo.CreateAsync(appointment);
                if (created == null)
                    return ServiceResponse<AppointmentDto>.FailureResponse(AppConstant.ErrorMessages.GenericError);

                // 9. Return DTO with action flags
                var dto2 = await _appointmentRepo.GetDtoByIdAsync(created.AppointmentId);
                if (dto2 == null)
                    return ServiceResponse<AppointmentDto>.FailureResponse(AppConstant.ErrorMessages.GenericError);

                SetActionFlags(dto2, userId, created.Status);
                _logger.LogInformation("Appointment {Id} scheduled by user {UserId} for proposal {ProposalId}", created.AppointmentId, userId, dto.ProposalId);
                return ServiceResponse<AppointmentDto>.SuccessResponse(dto2, AppConstant.SuccessMessages.AppointmentScheduled);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error scheduling appointment for user {UserId}", userId);
                return ServiceResponse<AppointmentDto>.FailureResponse(AppConstant.ErrorMessages.GenericError);
            }
        }

        public async Task<ServiceResponse<List<AppointmentDto>>> GetProposalAppointmentsAsync(string userId, int proposalId)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(userId))
                    return ServiceResponse<List<AppointmentDto>>.FailureResponse(AppConstant.ErrorMessages.UserIdRequired);

                var proposal = await _proposalRepo.GetEntityByIdAsync(proposalId);
                if (proposal == null)
                    return ServiceResponse<List<AppointmentDto>>.FailureResponse(AppConstant.ErrorMessages.ProposalNotFound);

                if (proposal.ProposerId != userId && proposal.RecipientId != userId)
                    return ServiceResponse<List<AppointmentDto>>.FailureResponse(AppConstant.ErrorMessages.UnauthorizedAppointmentAction);

                var appointments = await _appointmentRepo.GetByProposalIdAsync(proposalId);

                // Apply action flags based on status
                foreach (var a in appointments)
                {
                    var status = Enum.TryParse<AppointmentStatus>(a.Status, out var s) ? s : AppointmentStatus.Scheduled;
                    SetActionFlags(a, userId, status);
                }

                return ServiceResponse<List<AppointmentDto>>.SuccessResponse(appointments);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching appointments for proposal {ProposalId}", proposalId);
                return ServiceResponse<List<AppointmentDto>>.FailureResponse(AppConstant.ErrorMessages.GenericError);
            }
        }

        public async Task<ServiceResponse<AppointmentDto>> GetAppointmentAsync(string userId, int appointmentId)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(userId))
                    return ServiceResponse<AppointmentDto>.FailureResponse(AppConstant.ErrorMessages.UserIdRequired);

                var appointment = await _appointmentRepo.GetByIdAsync(appointmentId);
                if (appointment == null)
                    return ServiceResponse<AppointmentDto>.FailureResponse(AppConstant.ErrorMessages.AppointmentNotFound);

                var proposal = await _proposalRepo.GetEntityByIdAsync(appointment.ProposalId);
                if (proposal == null || (proposal.ProposerId != userId && proposal.RecipientId != userId))
                    return ServiceResponse<AppointmentDto>.FailureResponse(AppConstant.ErrorMessages.UnauthorizedAppointmentAction);

                var dto = await _appointmentRepo.GetDtoByIdAsync(appointmentId);
                if (dto == null)
                    return ServiceResponse<AppointmentDto>.FailureResponse(AppConstant.ErrorMessages.AppointmentNotFound);

                SetActionFlags(dto, userId, appointment.Status);
                return ServiceResponse<AppointmentDto>.SuccessResponse(dto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching appointment {AppointmentId}", appointmentId);
                return ServiceResponse<AppointmentDto>.FailureResponse(AppConstant.ErrorMessages.GenericError);
            }
        }

        public async Task<ServiceResponse<AppointmentDto>> CancelAppointmentAsync(string userId, int appointmentId)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(userId))
                    return ServiceResponse<AppointmentDto>.FailureResponse(AppConstant.ErrorMessages.UserIdRequired);

                var appointment = await _appointmentRepo.GetByIdAsync(appointmentId);
                if (appointment == null)
                    return ServiceResponse<AppointmentDto>.FailureResponse(AppConstant.ErrorMessages.AppointmentNotFound);

                if (appointment.Status == AppointmentStatus.Cancelled)
                    return ServiceResponse<AppointmentDto>.FailureResponse(AppConstant.ErrorMessages.AppointmentAlreadyCancelled);

                var proposal = await _proposalRepo.GetEntityByIdAsync(appointment.ProposalId);
                if (proposal == null || (proposal.ProposerId != userId && proposal.RecipientId != userId))
                    return ServiceResponse<AppointmentDto>.FailureResponse(AppConstant.ErrorMessages.UnauthorizedAppointmentAction);

                // Remove the Google Calendar event (best-effort — token may be revoked)
                if (!string.IsNullOrWhiteSpace(appointment.GoogleCalendarEventId))
                {
                    await _calendarService.CancelEventAsync(userId, appointment.GoogleCalendarEventId);
                }

                appointment.Status = AppointmentStatus.Cancelled;
                appointment.UpdatedAt = DateTime.UtcNow;
                await _appointmentRepo.UpdateAsync(appointment);

                var dto = await _appointmentRepo.GetDtoByIdAsync(appointmentId);
                if (dto != null) SetActionFlags(dto, userId, AppointmentStatus.Cancelled);

                _logger.LogInformation("Appointment {Id} cancelled by user {UserId}", appointmentId, userId);
                return ServiceResponse<AppointmentDto>.SuccessResponse(dto!, AppConstant.SuccessMessages.AppointmentCancelled);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cancelling appointment {AppointmentId}", appointmentId);
                return ServiceResponse<AppointmentDto>.FailureResponse(AppConstant.ErrorMessages.GenericError);
            }
        }

        public async Task<ServiceResponse<AppointmentDto>> RescheduleAppointmentAsync(
            string userId, int appointmentId, UpdateAppointmentDto dto)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(userId))
                    return ServiceResponse<AppointmentDto>.FailureResponse(AppConstant.ErrorMessages.UserIdRequired);

                // Fail-fast validation
                if (dto.MeetingTime.ToUniversalTime() <= DateTime.UtcNow.AddMinutes(15))
                    return ServiceResponse<AppointmentDto>.FailureResponse(AppConstant.ErrorMessages.InvalidMeetingTime);

                if (!AllowedDurations.Contains(dto.Duration))
                    return ServiceResponse<AppointmentDto>.FailureResponse(AppConstant.ErrorMessages.InvalidDuration);

                var appointment = await _appointmentRepo.GetByIdAsync(appointmentId);
                if (appointment == null)
                    return ServiceResponse<AppointmentDto>.FailureResponse(AppConstant.ErrorMessages.AppointmentNotFound);

                if (appointment.Status == AppointmentStatus.Cancelled)
                    return ServiceResponse<AppointmentDto>.FailureResponse(AppConstant.ErrorMessages.AppointmentAlreadyCancelled);

                var proposal = await _proposalRepo.GetEntityByIdAsync(appointment.ProposalId);
                if (proposal == null || (proposal.ProposerId != userId && proposal.RecipientId != userId))
                    return ServiceResponse<AppointmentDto>.FailureResponse(AppConstant.ErrorMessages.UnauthorizedAppointmentAction);

                // Check Google Calendar connection
                var calendarStatus = await _calendarService.GetStatusAsync(userId);
                if (!calendarStatus.Success || calendarStatus.Data == null || !calendarStatus.Data.IsConnected)
                    return ServiceResponse<AppointmentDto>.FailureResponse(
                        calendarStatus.Data?.IsRevoked == true
                            ? AppConstant.ErrorMessages.GoogleCalendarRevoked
                            : AppConstant.ErrorMessages.GoogleCalendarNotConnected);

                var newTimeUtc = DateTime.SpecifyKind(dto.MeetingTime.ToUniversalTime(), DateTimeKind.Utc);
                var description = dto.Description?.Trim();

                // Update Google Calendar event
                if (!string.IsNullOrWhiteSpace(appointment.GoogleCalendarEventId))
                {
                    var updateResult = await _calendarService.UpdateEventAsync(
                        userId, appointment.GoogleCalendarEventId, description, newTimeUtc, dto.Duration);

                    if (!updateResult.Success)
                        _logger.LogWarning("Could not update Google Calendar event {EventId}: {Msg}", appointment.GoogleCalendarEventId, updateResult.Message);
                }

                appointment.MeetingTime = newTimeUtc;
                appointment.Duration = dto.Duration;
                appointment.Description = description;
                appointment.Status = AppointmentStatus.Rescheduled;
                appointment.UpdatedAt = DateTime.UtcNow;

                await _appointmentRepo.UpdateAsync(appointment);

                var resultDto = await _appointmentRepo.GetDtoByIdAsync(appointmentId);
                if (resultDto != null) SetActionFlags(resultDto, userId, AppointmentStatus.Rescheduled);

                _logger.LogInformation("Appointment {Id} rescheduled by user {UserId}", appointmentId, userId);
                return ServiceResponse<AppointmentDto>.SuccessResponse(resultDto!, AppConstant.SuccessMessages.AppointmentRescheduled);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error rescheduling appointment {AppointmentId}", appointmentId);
                return ServiceResponse<AppointmentDto>.FailureResponse(AppConstant.ErrorMessages.GenericError);
            }
        }

        // ---- Helpers ---- //

        /// <summary>
        /// Sets CanCancel / CanReschedule based on current status.
        /// Both participants can cancel or reschedule as long as it isn't already cancelled.
        /// </summary>
        private static void SetActionFlags(AppointmentDto dto, string userId, AppointmentStatus status)
        {
            dto.CanCancel = status != AppointmentStatus.Cancelled;
            dto.CanReschedule = status != AppointmentStatus.Cancelled;
        }
    }
}
