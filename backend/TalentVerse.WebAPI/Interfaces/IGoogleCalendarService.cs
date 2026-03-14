using TalentVerse.WebAPI.Common;
using TalentVerse.WebAPI.DTO.Appointments;

namespace TalentVerse.WebAPI.Interfaces
{
    public interface IGoogleCalendarService
    {
        /// <summary>
        /// Generates the Google OAuth authorisation URL with the calendar.events scope.
        /// The <paramref name="userId"/> is encrypted into the OAuth state parameter to
        /// associate the callback with the correct user without storing server-side session.
        /// </summary>
        string GetAuthorizationUrl(string userId, string redirectUri);

        /// <summary>
        /// Exchanges the OAuth authorisation code for access/refresh tokens,
        /// encrypts them with Data Protection, and persists them in the database.
        /// Returns the Google account email on success.
        /// </summary>
        Task<ServiceResponse<string>> HandleCallbackAsync(string encryptedState, string authCode, string redirectUri);

        /// <summary>Returns the current Google Calendar connection status for the user.</summary>
        Task<ServiceResponse<GoogleCalendarStatusDto>> GetStatusAsync(string userId);

        /// <summary>Revokes the stored tokens and removes the record from the database.</summary>
        Task<ServiceResponse<bool>> DisconnectAsync(string userId);

        /// <summary>
        /// Creates a Google Calendar event on the scheduler's primary calendar with:
        ///   • A Google Meet video link (via ConferenceData.CreateRequest)
        ///   • The other participant added as an attendee (they receive an invite email)
        /// Returns the calendar event ID and the generated Meet link.
        /// 
        /// Handles token revocation: if the Google API responds with invalid_grant,
        /// the stored token is marked as revoked and a GoogleCalendarRevoked error is returned.
        /// </summary>
        Task<ServiceResponse<(string EventId, string MeetLink)>> CreateEventAsync(
            string userId,
            string attendeeEmail,
            string eventTitle,
            string? description,
            DateTime startUtc,
            int durationMinutes);

        /// <summary>
        /// Updates an existing calendar event (reschedule — new time/duration/description).
        /// Handles token revocation like CreateEventAsync.
        /// </summary>
        Task<ServiceResponse<string>> UpdateEventAsync(
            string userId,
            string googleEventId,
            string? description,
            DateTime newStartUtc,
            int newDurationMinutes);

        /// <summary>
        /// Deletes a calendar event. Used when an appointment is cancelled.
        /// Handles token revocation like CreateEventAsync.
        /// </summary>
        Task<ServiceResponse<bool>> CancelEventAsync(string userId, string googleEventId);
    }
}
