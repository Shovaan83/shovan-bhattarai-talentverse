using TalentVerse.WebAPI.Common;
using TalentVerse.WebAPI.Data.Entities;
using TalentVerse.WebAPI.DTO.Appointments;

namespace TalentVerse.WebAPI.Interfaces
{
    public interface IAppointmentRepository
    {
        /// <summary>Persists a new appointment.</summary>
        Task<Appointment?> CreateAsync(Appointment appointment);

        /// <summary>Returns the raw entity (for state checks before updates).</summary>
        Task<Appointment?> GetByIdAsync(int appointmentId);

        /// <summary>Returns all appointments for a proposal, newest first.</summary>
        Task<List<AppointmentDto>> GetByProposalIdAsync(int proposalId);

        /// <summary>Returns a single appointment as a mapped DTO with creator username.</summary>
        Task<AppointmentDto?> GetDtoByIdAsync(int appointmentId);

        /// <summary>Persists updated fields (MeetingTime, Duration, Description, Status, MeetingLink, GoogleCalendarEventId, UpdatedAt).</summary>
        Task<bool> UpdateAsync(Appointment appointment);
    }
}
