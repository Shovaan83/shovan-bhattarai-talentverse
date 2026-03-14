using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TalentVerse.WebAPI.Data.Entities
{
    /// <summary>
    /// Stores the encrypted Google OAuth tokens needed to interact with the
    /// Google Calendar API on behalf of a user.
    /// Tokens are encrypted at rest using ASP.NET Core Data Protection.
    /// </summary>
    public class GoogleCalendarToken
    {
        [Key]
        public int TokenId { get; set; }

        /// <summary>One token record per user (unique index enforced in OnModelCreating).</summary>
        [Required]
        public string UserId { get; set; } = string.Empty;
        [ForeignKey("UserId")]
        public virtual AppUser User { get; set; }

        /// <summary>Encrypted access token (Data Protection API). May be longer than raw due to encryption overhead.</summary>
        [Required]
        [MaxLength(8192)]
        public string AccessToken { get; set; } = string.Empty;

        /// <summary>Encrypted refresh token. Never expires unless explicitly revoked by the user.</summary>
        [Required]
        [MaxLength(8192)]
        public string RefreshToken { get; set; } = string.Empty;

        /// <summary>UTC expiry of the current access token (typically 1 hour after issue).</summary>
        public DateTime TokenExpiry { get; set; }

        /// <summary>Space-separated list of granted OAuth scopes.</summary>
        [MaxLength(2048)]
        public string Scopes { get; set; } = string.Empty;

        /// <summary>Google account email shown to the user in the "Connected" UI.</summary>
        [MaxLength(500)]
        public string? GoogleEmail { get; set; }

        /// <summary>
        /// Marks the token as revoked. Set to TRUE when the Google API returns
        /// invalid_grant (the user revoked access from their Google account settings).
        /// The scheduler UI should prompt the user to reconnect when this is true.
        /// </summary>
        public bool IsRevoked { get; set; } = false;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
