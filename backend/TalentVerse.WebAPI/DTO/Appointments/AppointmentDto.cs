namespace TalentVerse.WebAPI.DTO.Appointments
{
    /// <summary>
    /// Full appointment view returned by the API.
    /// All DateTime values are UTC — the frontend is responsible for
    /// converting them to the user's local timezone for display.
    /// </summary>
    public class AppointmentDto
    {
        public int AppointmentId { get; set; }
        public int ProposalId { get; set; }

        public string CreatedByUserId { get; set; } = string.Empty;
        public string CreatedByUsername { get; set; } = string.Empty;

        /// <summary>Meeting start time in UTC.</summary>
        public DateTime MeetingTime { get; set; }

        /// <summary>Duration in minutes (30 / 60 / 90 / 120).</summary>
        public int Duration { get; set; }

        public string? Description { get; set; }

        /// <summary>Google Meet link — open directly in browser.</summary>
        public string? MeetingLink { get; set; }

        /// <summary>"Scheduled", "Cancelled", or "Rescheduled".</summary>
        public string Status { get; set; } = "Scheduled";

        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        // ---- Action flags computed by AppointmentService ----
        /// <summary>True when the current user can cancel this appointment.</summary>
        public bool CanCancel { get; set; }

        /// <summary>True when the current user can reschedule this appointment.</summary>
        public bool CanReschedule { get; set; }
    }
}
