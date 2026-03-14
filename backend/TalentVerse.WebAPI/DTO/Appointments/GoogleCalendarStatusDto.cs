namespace TalentVerse.WebAPI.DTO.Appointments
{
    /// <summary>
    /// Returned by GET /api/appointments/google-calendar/status.
    /// Tells the frontend whether the current user has a connected (and non-revoked)
    /// Google Calendar token, plus which Google account is linked.
    /// </summary>
    public class GoogleCalendarStatusDto
    {
        public bool IsConnected { get; set; }

        /// <summary>
        /// The Google account email stored when the user authorised calendar access.
        /// Null when IsConnected is false.
        /// </summary>
        public string? GoogleEmail { get; set; }

        /// <summary>
        /// True when the token exists but is marked revoked (user removed TalentVerse
        /// from their Google account permissions). The frontend should prompt reconnection.
        /// </summary>
        public bool IsRevoked { get; set; }
    }
}
