using System.ComponentModel.DataAnnotations;

namespace TalentVerse.WebAPI.DTO.Appointments
{
    public class CreateAppointmentDto
    {
        [Required]
        public int ProposalId { get; set; }

        /// <summary>
        /// Meeting start time submitted as UTC from the frontend.
        /// The frontend must convert the user's local datetime-local input to UTC
        /// before sending (e.g. new Date(localStr).toISOString()).
        /// Must be a future timestamp.
        /// </summary>
        [Required]
        public DateTime MeetingTime { get; set; }

        /// <summary>Duration in minutes. Allowed values: 30, 60, 90, 120.</summary>
        [Required]
        [Range(30, 120)]
        public int Duration { get; set; } = 60;

        /// <summary>Optional agenda description shown on the calendar event (max 500 chars).</summary>
        [MaxLength(500)]
        public string? Description { get; set; }
    }
}
