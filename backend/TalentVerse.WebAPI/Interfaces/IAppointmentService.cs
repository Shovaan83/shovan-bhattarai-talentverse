using TalentVerse.WebAPI.Common;
using TalentVerse.WebAPI.DTO.Appointments;

namespace TalentVerse.WebAPI.Interfaces
{
    public interface IAppointmentService
    {
        /// <summary>Schedules a meeting for an accepted proposal and creates the Google Calendar event.</summary>
        Task<ServiceResponse<AppointmentDto>> ScheduleAppointmentAsync(string userId, CreateAppointmentDto dto);

        /// <summary>Returns all appointments for a proposal visible to the current user.</summary>
        Task<ServiceResponse<List<AppointmentDto>>> GetProposalAppointmentsAsync(string userId, int proposalId);

        /// <summary>Returns a single appointment by ID.</summary>
        Task<ServiceResponse<AppointmentDto>> GetAppointmentAsync(string userId, int appointmentId);

        /// <summary>Cancels the appointment and deletes the corresponding Google Calendar event.</summary>
        Task<ServiceResponse<AppointmentDto>> CancelAppointmentAsync(string userId, int appointmentId);

        /// <summary>Reschedules the appointment and updates the Google Calendar event.</summary>
        Task<ServiceResponse<AppointmentDto>> RescheduleAppointmentAsync(string userId, int appointmentId, UpdateAppointmentDto dto);
    }
}
