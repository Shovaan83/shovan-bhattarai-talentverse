using System.ComponentModel.DataAnnotations;

namespace TalentVerse.WebAPI.DTO.Appointments
{
    public class UpdateAppointmentDto
    {
        /// <summary>New meeting start time in UTC. Must be in the future.</summary>
        [Required]
        public DateTime MeetingTime { get; set; }

        /// <summary>Duration in minutes. Allowed values: 30, 60, 90, 120.</summary>
        [Required]
        [Range(30, 120)]
        public int Duration { get; set; } = 60;

        [MaxLength(500)]
        public string? Description { get; set; }
    }
}
