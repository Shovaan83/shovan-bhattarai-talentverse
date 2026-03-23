using Dapper;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Auth.OAuth2.Flows;
using Google.Apis.Auth.OAuth2.Responses;
using Google.Apis.Calendar.v3;
using Google.Apis.Calendar.v3.Data;
using Google.Apis.Services;
using Microsoft.AspNetCore.DataProtection;
using TalentVerse.WebAPI.Common;
using TalentVerse.WebAPI.Data;
using TalentVerse.WebAPI.Data.Entities;
using TalentVerse.WebAPI.DTO.Appointments;
using TalentVerse.WebAPI.Interfaces;

namespace TalentVerse.WebAPI.Services
{
    public class GoogleCalendarService : IGoogleCalendarService
    {
        private const string ProtectorPurpose = "GoogleCalendarTokens";
        // The calendar.events scope allows creating/editing/deleting events (not full calendar access)
        private static readonly string CalendarScope = CalendarService.Scope.CalendarEvents;

        private readonly DapperContext _db;
        private readonly IDataProtector _protector;
        private readonly ILogger<GoogleCalendarService> _logger;
        private readonly string _clientId;
        private readonly string _clientSecret;

        public GoogleCalendarService(
            DapperContext db,
            IDataProtectionProvider dataProtectionProvider,
            IConfiguration configuration,
            ILogger<GoogleCalendarService> logger)
        {
            _db = db;
            _protector = dataProtectionProvider.CreateProtector(ProtectorPurpose);
            _logger = logger;
            _clientId = configuration["Authentication:Google:ClientId"]
                        ?? throw new InvalidOperationException("Authentication:Google:ClientId is not configured.");
            _clientSecret = configuration["Authentication:Google:ClientSecret"]
                            ?? throw new InvalidOperationException("Authentication:Google:ClientSecret is not configured.");
        }

        // ------------------------------------------------------------------ //
        //  Public API                                                          //
        // ------------------------------------------------------------------ //

        public string GetAuthorizationUrl(string userId, string redirectUri)
        {
            // Encrypt userId into the OAuth state parameter to prevent CSRF and
            // to associate the callback with the correct user without server-side session.
            var state = _protector.Protect(userId);
            var flow = BuildFlow(string.Empty);
            var request = flow.CreateAuthorizationCodeRequest(redirectUri);
            request.State = state;
            return request.Build().ToString();
        }

        public async Task<ServiceResponse<string>> HandleCallbackAsync(
            string encryptedState, string authCode, string redirectUri)
        {
            string userId;
            try
            {
                userId = _protector.Unprotect(encryptedState);
            }
            catch
            {
                return ServiceResponse<string>.FailureResponse("Invalid OAuth state. Please try connecting again.");
            }

            try
            {
                var flow = BuildFlow(redirectUri);
                var tokenResponse = await flow.ExchangeCodeForTokenAsync(userId, authCode, redirectUri, CancellationToken.None);

                // Fetch Google account email to show in the UI
                var googleEmail = await GetGoogleEmailAsync(tokenResponse.AccessToken);

                await PersistTokenAsync(userId, tokenResponse, googleEmail);

                _logger.LogInformation("Google Calendar connected for user {UserId} ({Email})", userId, googleEmail);
                return ServiceResponse<string>.SuccessResponse(googleEmail ?? string.Empty, AppConstant.SuccessMessages.GoogleCalendarConnected);
            }
            catch (TokenResponseException ex)
            {
                _logger.LogWarning(ex, "Token exchange failed for user {UserId}: {Error}", userId, ex.Error?.Error);
                return ServiceResponse<string>.FailureResponse("Failed to exchange authorisation code. Please try again.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during Google Calendar callback for user {UserId}", userId);
                return ServiceResponse<string>.FailureResponse(AppConstant.ErrorMessages.GenericError);
            }
        }

        public async Task<ServiceResponse<GoogleCalendarStatusDto>> GetStatusAsync(string userId)
        {
            try
            {
                using var connection = _db.CreateConnection();
                var token = await connection.QuerySingleOrDefaultAsync<GoogleCalendarToken>(
                    @"SELECT * FROM ""GoogleCalendarTokens"" WHERE ""UserId"" = @UserId",
                    new { UserId = userId });

                if (token == null)
                    return ServiceResponse<GoogleCalendarStatusDto>.SuccessResponse(new GoogleCalendarStatusDto { IsConnected = false });

                return ServiceResponse<GoogleCalendarStatusDto>.SuccessResponse(new GoogleCalendarStatusDto
                {
                    IsConnected = !token.IsRevoked,
                    GoogleEmail = token.GoogleEmail,
                    IsRevoked = token.IsRevoked
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching Google Calendar status for user {UserId}", userId);
                return ServiceResponse<GoogleCalendarStatusDto>.FailureResponse(AppConstant.ErrorMessages.GenericError);
            }
        }

        public async Task<ServiceResponse<bool>> DisconnectAsync(string userId)
        {
            try
            {
                using var connection = _db.CreateConnection();
                var token = await connection.QuerySingleOrDefaultAsync<GoogleCalendarToken>(
                    @"SELECT * FROM ""GoogleCalendarTokens"" WHERE ""UserId"" = @UserId",
                    new { UserId = userId });

                if (token == null)
                    return ServiceResponse<bool>.SuccessResponse(true, AppConstant.SuccessMessages.GoogleCalendarDisconnected);

                // Attempt to revoke the token at Google (best-effort — don't fail if this errors)
                try
                {
                    var decryptedAccessToken = _protector.Unprotect(token.AccessToken);
                    var revokeUrl = $"https://oauth2.googleapis.com/revoke?token={Uri.EscapeDataString(decryptedAccessToken)}";
                    using var httpClient = new HttpClient();
                    await httpClient.PostAsync(revokeUrl, null);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Could not revoke Google token for user {UserId} — removing locally anyway", userId);
                }

                await connection.ExecuteAsync(
                    @"DELETE FROM ""GoogleCalendarTokens"" WHERE ""UserId"" = @UserId",
                    new { UserId = userId });

                _logger.LogInformation("Google Calendar disconnected for user {UserId}", userId);
                return ServiceResponse<bool>.SuccessResponse(true, AppConstant.SuccessMessages.GoogleCalendarDisconnected);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error disconnecting Google Calendar for user {UserId}", userId);
                return ServiceResponse<bool>.FailureResponse(AppConstant.ErrorMessages.GenericError);
            }
        }

        public async Task<ServiceResponse<(string EventId, string MeetLink)>> CreateEventAsync(
            string userId,
            string attendeeEmail,
            string eventTitle,
            string? description,
            DateTime startUtc,
            int durationMinutes)
        {
            try
            {
                var service = await GetCalendarServiceAsync(userId);

                var newEvent = BuildCalendarEvent(eventTitle, description, startUtc, durationMinutes, attendeeEmail);

                var request = service.Events.Insert(newEvent, "primary");
                request.ConferenceDataVersion = 1; // Required so Google attaches a Meet link

                var createdEvent = await request.ExecuteAsync();

                var meetLink = createdEvent.ConferenceData?.EntryPoints
                                   ?.FirstOrDefault(ep => ep.EntryPointType == "video")?.Uri
                               ?? string.Empty;

                return ServiceResponse<(string, string)>.SuccessResponse(
                    (createdEvent.Id, meetLink),
                    AppConstant.SuccessMessages.AppointmentScheduled);
            }
            catch (InvalidOperationException ex) when (ex.Message is "GoogleCalendarNotConnected" or "GoogleCalendarRevoked")
            {
                var msg = ex.Message == "GoogleCalendarRevoked"
                    ? AppConstant.ErrorMessages.GoogleCalendarRevoked
                    : AppConstant.ErrorMessages.GoogleCalendarNotConnected;
                return ServiceResponse<(string, string)>.FailureResponse(msg);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create Google Calendar event for user {UserId}", userId);
                return ServiceResponse<(string, string)>.FailureResponse(AppConstant.ErrorMessages.GoogleCalendarError);
            }
        }

        public async Task<ServiceResponse<string>> UpdateEventAsync(
            string userId,
            string googleEventId,
            string? description,
            DateTime newStartUtc,
            int newDurationMinutes)
        {
            try
            {
                var service = await GetCalendarServiceAsync(userId);

                // Fetch the existing event so we preserve attendees and other fields
                var existing = await service.Events.Get("primary", googleEventId).ExecuteAsync();

                existing.Start = new EventDateTime { DateTimeDateTimeOffset = DateTime.SpecifyKind(newStartUtc, DateTimeKind.Utc) };
                existing.End = new EventDateTime { DateTimeDateTimeOffset = DateTime.SpecifyKind(newStartUtc.AddMinutes(newDurationMinutes), DateTimeKind.Utc) };
                if (description != null) existing.Description = description;

                var updated = await service.Events.Update(existing, "primary", googleEventId).ExecuteAsync();
                return ServiceResponse<string>.SuccessResponse(updated.Id, AppConstant.SuccessMessages.AppointmentRescheduled);
            }
            catch (InvalidOperationException ex) when (ex.Message is "GoogleCalendarNotConnected" or "GoogleCalendarRevoked")
            {
                var msg = ex.Message == "GoogleCalendarRevoked"
                    ? AppConstant.ErrorMessages.GoogleCalendarRevoked
                    : AppConstant.ErrorMessages.GoogleCalendarNotConnected;
                return ServiceResponse<string>.FailureResponse(msg);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to update Google Calendar event {EventId} for user {UserId}", googleEventId, userId);
                return ServiceResponse<string>.FailureResponse(AppConstant.ErrorMessages.GoogleCalendarError);
            }
        }

        public async Task<ServiceResponse<bool>> CancelEventAsync(string userId, string googleEventId)
        {
            try
            {
                var service = await GetCalendarServiceAsync(userId);
                await service.Events.Delete("primary", googleEventId).ExecuteAsync();
                return ServiceResponse<bool>.SuccessResponse(true);
            }
            catch (InvalidOperationException ex) when (ex.Message is "GoogleCalendarNotConnected" or "GoogleCalendarRevoked")
            {
                // If the token is revoked we cannot cancel on Google, but we still want the
                // local appointment to be marked Cancelled — so return success here and let
                // the service layer handle the DB update regardless.
                _logger.LogWarning("Could not cancel Google event {EventId} for user {UserId}: {Msg}", googleEventId, userId, ex.Message);
                return ServiceResponse<bool>.SuccessResponse(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to cancel Google Calendar event {EventId} for user {UserId}", googleEventId, userId);
                // Best-effort: still return success — the local appointment will be cancelled
                return ServiceResponse<bool>.SuccessResponse(true);
            }
        }

        // ------------------------------------------------------------------ //
        //  Private helpers                                                     //
        // ------------------------------------------------------------------ //

        /// <summary>
        /// Returns a CalendarService backed by the user's stored (and auto-refreshed) tokens.
        /// Throws InvalidOperationException("GoogleCalendarNotConnected") when no token exists.
        /// Throws InvalidOperationException("GoogleCalendarRevoked") when the refresh token is
        /// invalid (user removed TalentVerse access from their Google account settings).
        /// The IsRevoked flag is written to the database before throwing.
        /// </summary>
        private async Task<CalendarService> GetCalendarServiceAsync(string userId)
        {
            using var connection = _db.CreateConnection();
            connection.Open();

            var token = await connection.QuerySingleOrDefaultAsync<GoogleCalendarToken>(
                @"SELECT * FROM ""GoogleCalendarTokens"" WHERE ""UserId"" = @UserId AND ""IsRevoked"" = FALSE",
                new { UserId = userId });

            if (token == null)
                throw new InvalidOperationException("GoogleCalendarNotConnected");

            var decryptedAccessToken = _protector.Unprotect(token.AccessToken);
            var decryptedRefreshToken = _protector.Unprotect(token.RefreshToken);

            // Refresh the access token when it is expired or within 5 minutes of expiry
            if (token.TokenExpiry <= DateTime.UtcNow.AddMinutes(5))
            {
                try
                {
                    var flow = BuildFlow(string.Empty);
                    var refreshed = await flow.RefreshTokenAsync(userId, decryptedRefreshToken, CancellationToken.None);

                    decryptedAccessToken = refreshed.AccessToken;

                    var newExpiry = DateTime.UtcNow.AddSeconds(refreshed.ExpiresInSeconds ?? 3600);
                    var encryptedNewToken = _protector.Protect(refreshed.AccessToken);

                    await connection.ExecuteAsync(
                        @"UPDATE ""GoogleCalendarTokens""
                          SET    ""AccessToken"" = @AccessToken,
                                 ""TokenExpiry""  = @TokenExpiry,
                                 ""UpdatedAt""    = @UpdatedAt
                          WHERE  ""UserId"" = @UserId",
                        new { AccessToken = encryptedNewToken, TokenExpiry = newExpiry, UpdatedAt = DateTime.UtcNow, UserId = userId });

                    _logger.LogInformation("Google Calendar token refreshed for user {UserId}", userId);
                }
                catch (TokenResponseException ex)
                {
                    // invalid_grant means the user revoked access from their Google account.
                    _logger.LogWarning("Google Calendar token revoked for user {UserId}: {Error}", userId, ex.Error?.Error);

                    await connection.ExecuteAsync(
                        @"UPDATE ""GoogleCalendarTokens""
                          SET    ""IsRevoked"" = TRUE, ""UpdatedAt"" = @UpdatedAt
                          WHERE  ""UserId"" = @UserId",
                        new { UpdatedAt = DateTime.UtcNow, UserId = userId });

                    throw new InvalidOperationException("GoogleCalendarRevoked");
                }
            }

            var credential = GoogleCredential.FromAccessToken(decryptedAccessToken);
            return new CalendarService(new BaseClientService.Initializer
            {
                HttpClientInitializer = credential,
                ApplicationName = "TalentVerse"
            });
        }

        private GoogleAuthorizationCodeFlow BuildFlow(string redirectUri) =>
            new(new GoogleAuthorizationCodeFlow.Initializer
            {
                ClientSecrets = new ClientSecrets { ClientId = _clientId, ClientSecret = _clientSecret },
                Scopes = new[] { CalendarScope, "email", "profile" }
            });

        private static Event BuildCalendarEvent(
            string title, string? description, DateTime startUtc, int durationMinutes, string attendeeEmail)
        {
            var start = DateTime.SpecifyKind(startUtc, DateTimeKind.Utc);
            var end = start.AddMinutes(durationMinutes);

            return new Event
            {
                Summary = title,
                Description = string.IsNullOrWhiteSpace(description)
                    ? "Scheduled via TalentVerse"
                    : $"{description}\n\nScheduled via TalentVerse",
                Start = new EventDateTime { DateTimeDateTimeOffset = start },
                End = new EventDateTime { DateTimeDateTimeOffset = end },
                Attendees = new List<EventAttendee>
                {
                    new() { Email = attendeeEmail }
                },
                ConferenceData = new ConferenceData
                {
                    CreateRequest = new CreateConferenceRequest
                    {
                        RequestId = Guid.NewGuid().ToString(),
                        ConferenceSolutionKey = new ConferenceSolutionKey { Type = "hangoutsMeet" }
                    }
                },
                // Send email invites to attendees
                GuestsCanSeeOtherGuests = true
            };
        }

        private async Task PersistTokenAsync(string userId, TokenResponse tokenResponse, string? googleEmail)
        {
            using var connection = _db.CreateConnection();
            connection.Open();

            var expiry = DateTime.UtcNow.AddSeconds(tokenResponse.ExpiresInSeconds ?? 3600);
            var encryptedAccess = _protector.Protect(tokenResponse.AccessToken);
            var encryptedRefresh = _protector.Protect(tokenResponse.RefreshToken);
            var scopes = tokenResponse.Scope ?? CalendarScope;

            // Upsert: update if exists, insert if not
            var existing = await connection.QuerySingleOrDefaultAsync<int?>(
                @"SELECT ""TokenId"" FROM ""GoogleCalendarTokens"" WHERE ""UserId"" = @UserId",
                new { UserId = userId });

            if (existing.HasValue)
            {
                await connection.ExecuteAsync(
                    @"UPDATE ""GoogleCalendarTokens""
                      SET    ""AccessToken""  = @AccessToken,
                             ""RefreshToken"" = @RefreshToken,
                             ""TokenExpiry""  = @TokenExpiry,
                             ""Scopes""       = @Scopes,
                             ""GoogleEmail""  = @GoogleEmail,
                             ""IsRevoked""    = FALSE,
                             ""UpdatedAt""    = @UpdatedAt
                      WHERE  ""UserId"" = @UserId",
                    new { AccessToken = encryptedAccess, RefreshToken = encryptedRefresh, TokenExpiry = expiry, Scopes = scopes, GoogleEmail = googleEmail, UpdatedAt = DateTime.UtcNow, UserId = userId });
            }
            else
            {
                await connection.ExecuteAsync(
                    @"INSERT INTO ""GoogleCalendarTokens""
                             (""UserId"", ""AccessToken"", ""RefreshToken"", ""TokenExpiry"", ""Scopes"", ""GoogleEmail"", ""IsRevoked"", ""CreatedAt"", ""UpdatedAt"")
                      VALUES (@UserId,    @AccessToken,    @RefreshToken,    @TokenExpiry,    @Scopes,    @GoogleEmail,    FALSE,           @Now,          @Now)",
                    new { UserId = userId, AccessToken = encryptedAccess, RefreshToken = encryptedRefresh, TokenExpiry = expiry, Scopes = scopes, GoogleEmail = googleEmail, Now = DateTime.UtcNow });
            }
        }

        private static async Task<string?> GetGoogleEmailAsync(string accessToken)
        {
            try
            {
                using var httpClient = new HttpClient();
                httpClient.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);

                var response = await httpClient.GetAsync("https://www.googleapis.com/oauth2/v2/userinfo");
                if (!response.IsSuccessStatusCode) return null;

                var json = System.Text.Json.JsonDocument.Parse(await response.Content.ReadAsStringAsync());
                return json.RootElement.TryGetProperty("email", out var emailEl) ? emailEl.GetString() : null;
            }
            catch
            {
                return null;
            }
        }
    }
}
