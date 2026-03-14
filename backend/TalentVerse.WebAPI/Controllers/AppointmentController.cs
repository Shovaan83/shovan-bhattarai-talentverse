using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using System.Security.Claims;
using TalentVerse.WebAPI.Common;
using TalentVerse.WebAPI.DTO.Appointments;
using TalentVerse.WebAPI.Interfaces;

namespace TalentVerse.WebAPI.Controllers
{
    [ApiController]
    [Route("api/appointments")]
    public class AppointmentController : ControllerBase
    {
        private readonly IAppointmentService _appointmentService;
        private readonly IGoogleCalendarService _calendarService;
        private readonly IConfiguration _configuration;

        public AppointmentController(
            IAppointmentService appointmentService,
            IGoogleCalendarService calendarService,
            IConfiguration configuration)
        {
            _appointmentService = appointmentService;
            _calendarService = calendarService;
            _configuration = configuration;
        }

        // =====================================================================
        // Google Calendar OAuth
        // =====================================================================

        /// <summary>Returns whether the current user has a connected Google Calendar.</summary>
        [Authorize]
        [HttpGet("google-calendar/status")]
        [ProducesResponseType(typeof(ServiceResponse<GoogleCalendarStatusDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<ServiceResponse<GoogleCalendarStatusDto>>> GetCalendarStatus()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(userId))
                return Unauthorized(ServiceResponse<GoogleCalendarStatusDto>.FailureResponse("Not authenticated."));

            var result = await _calendarService.GetStatusAsync(userId);
            return Ok(result);
        }

        /// <summary>
        /// Returns the Google OAuth authorization URL for the frontend to navigate to.
        /// Works for all account types (email/password, GitHub, or Google sign-in).
        /// The frontend calls this via axios (JWT bearer), then does window.location.href = url.
        /// </summary>
        [Authorize]
        [HttpGet("google-calendar/connect")]
        [ProducesResponseType(typeof(ServiceResponse<string>), StatusCodes.Status200OK)]
        public IActionResult ConnectGoogleCalendar()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(userId))
                return Unauthorized();

            var redirectUri = BuildCallbackUri();
            var authUrl = _calendarService.GetAuthorizationUrl(userId, redirectUri);
            return Ok(ServiceResponse<string>.SuccessResponse(authUrl, "Authorization URL generated"));
        }

        /// <summary>
        /// Handles the OAuth redirect from Google after the user grants (or denies) access.
        /// On success, redirects to the frontend settings page with a success flag.
        /// </summary>
        [HttpGet("google-calendar/callback")]
        public async Task<IActionResult> GoogleCalendarCallback([FromQuery] string? code, [FromQuery] string? state, [FromQuery] string? error)
        {
            var frontendUrl = _configuration["FrontendUrl"] ?? "http://localhost:3000";

            if (!string.IsNullOrWhiteSpace(error) || string.IsNullOrWhiteSpace(code) || string.IsNullOrWhiteSpace(state))
                return Redirect($"{frontendUrl}/dashboard?calendarConnected=false&error={Uri.EscapeDataString(error ?? "access_denied")}");

            var redirectUri = BuildCallbackUri();
            var result = await _calendarService.HandleCallbackAsync(state, code, redirectUri);

            if (!result.Success)
                return Redirect($"{frontendUrl}/dashboard?calendarConnected=false");

            return Redirect($"{frontendUrl}/dashboard?calendarConnected=true");
        }

        /// <summary>Disconnects the user's Google Calendar and revokes stored tokens.</summary>
        [Authorize]
        [HttpDelete("google-calendar/disconnect")]
        [ProducesResponseType(typeof(ServiceResponse<bool>), StatusCodes.Status200OK)]
        public async Task<ActionResult<ServiceResponse<bool>>> DisconnectGoogleCalendar()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(userId)) return Unauthorized();

            var result = await _calendarService.DisconnectAsync(userId);
            return Ok(result);
        }

        // =====================================================================
        // Appointments
        // =====================================================================

        /// <summary>
        /// Schedules a meeting for an accepted proposal and creates a Google Calendar
        /// event with a Google Meet link. Both participants receive calendar invites.
        /// </summary>
        [Authorize]
        [HttpPost]
        [EnableRateLimiting("fixed")]
        [ProducesResponseType(typeof(ServiceResponse<AppointmentDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ServiceResponse<AppointmentDto>), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<ServiceResponse<AppointmentDto>>> ScheduleAppointment([FromBody] CreateAppointmentDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ServiceResponse<AppointmentDto>.FailureResponse("Validation failed."));

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var result = await _appointmentService.ScheduleAppointmentAsync(userId!, dto);

            return result.Success ? Ok(result) : BadRequest(result);
        }

        /// <summary>Returns all appointments for a proposal (proposer and recipient only).</summary>
        [Authorize]
        [HttpGet("proposal/{proposalId:int}")]
        [ProducesResponseType(typeof(ServiceResponse<List<AppointmentDto>>), StatusCodes.Status200OK)]
        public async Task<ActionResult<ServiceResponse<List<AppointmentDto>>>> GetProposalAppointments(int proposalId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var result = await _appointmentService.GetProposalAppointmentsAsync(userId!, proposalId);

            return result.Success ? Ok(result) : BadRequest(result);
        }

        /// <summary>Returns a single appointment by ID.</summary>
        [Authorize]
        [HttpGet("{id:int}")]
        [ProducesResponseType(typeof(ServiceResponse<AppointmentDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ServiceResponse<AppointmentDto>), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ServiceResponse<AppointmentDto>>> GetAppointment(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var result = await _appointmentService.GetAppointmentAsync(userId!, id);

            if (!result.Success) return NotFound(result);
            return Ok(result);
        }

        /// <summary>Cancels an appointment and removes the Google Calendar event.</summary>
        [Authorize]
        [HttpPatch("{id:int}/cancel")]
        [EnableRateLimiting("fixed")]
        [ProducesResponseType(typeof(ServiceResponse<AppointmentDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ServiceResponse<AppointmentDto>), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<ServiceResponse<AppointmentDto>>> CancelAppointment(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var result = await _appointmentService.CancelAppointmentAsync(userId!, id);

            return result.Success ? Ok(result) : BadRequest(result);
        }

        /// <summary>Reschedules an appointment and updates the Google Calendar event.</summary>
        [Authorize]
        [HttpPut("{id:int}")]
        [EnableRateLimiting("fixed")]
        [ProducesResponseType(typeof(ServiceResponse<AppointmentDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ServiceResponse<AppointmentDto>), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<ServiceResponse<AppointmentDto>>> RescheduleAppointment(int id, [FromBody] UpdateAppointmentDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ServiceResponse<AppointmentDto>.FailureResponse("Validation failed."));

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var result = await _appointmentService.RescheduleAppointmentAsync(userId!, id, dto);

            return result.Success ? Ok(result) : BadRequest(result);
        }

        // ---- Private helpers ---- //

        private string BuildCallbackUri()
        {
            var request = HttpContext.Request;
            return $"{request.Scheme}://{request.Host}/api/appointments/google-calendar/callback";
        }
    }
}
