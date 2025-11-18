using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TalentVerse.WebAPI.Data.Entities
{
    public class Appointment
    {
        [Key]
        public int AppointmentId { get; set; }

        [Required]
        public int ProposalId { get; set; }
        [ForeignKey("ProposalId")]
        public virtual Proposal Proposal { get; set; }

        public DateTime MeetingTime { get; set; }

        [MaxLength(2048)]
        public string? MeetingLink { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}