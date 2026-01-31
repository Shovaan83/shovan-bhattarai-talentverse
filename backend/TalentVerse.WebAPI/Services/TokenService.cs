using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using TalentVerse.WebAPI.Data.Entities;
using TalentVerse.WebAPI.Interfaces;

namespace TalentVerse.WebAPI.Services
{
    public class TokenService : ITokenService
    {
        private readonly IConfiguration _config;
        private readonly SymmetricSecurityKey _key;
        private readonly UserManager<AppUser> _userManager;
        private readonly ILogger<TokenService> _logger;

        public TokenService(IConfiguration config, UserManager<AppUser> userManager, ILogger<TokenService> logger)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
            _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            // Guard clause: validate JWT configuration
            var tokenKey = _config["JWT:TokenKey"];
            if (string.IsNullOrWhiteSpace(tokenKey))
                throw new InvalidOperationException("JWT:TokenKey is not configured.");

            if (tokenKey.Length < 32)
                throw new InvalidOperationException("JWT:TokenKey must be at least 32 characters for security.");

            _key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(tokenKey));
        }

        public async Task<string> CreateToken(AppUser user)
        {
            // Guard clause: null user
            if (user is null)
            {
                _logger.LogError("{Method} called with null user", nameof(CreateToken));
                throw new ArgumentNullException(nameof(user), "User cannot be null.");
            }

            // Guard clause: validate user properties
            if (string.IsNullOrWhiteSpace(user.Email))
            {
                _logger.LogError("{Method} called for user {UserId} with missing email", nameof(CreateToken), user.Id);
                throw new InvalidOperationException("User email is required to create a token.");
            }

            if (string.IsNullOrWhiteSpace(user.UserName))
            {
                _logger.LogError("{Method} called for user {UserId} with missing username", nameof(CreateToken), user.Id);
                throw new InvalidOperationException("User username is required to create a token.");
            }

            try
            {
                var claims = new List<Claim>
                {
                    new Claim(JwtRegisteredClaimNames.Email, user.Email),
                    new Claim(JwtRegisteredClaimNames.NameId, user.Id),
                    new Claim(JwtRegisteredClaimNames.UniqueName, user.UserName)
                };

                var roles = await _userManager.GetRolesAsync(user);

                foreach (var role in roles)
                {
                    claims.Add(new Claim(ClaimTypes.Role, role));
                }

                var creds = new SigningCredentials(_key, SecurityAlgorithms.HmacSha512Signature);

                var tokenDescriptor = new SecurityTokenDescriptor
                {
                    Subject = new ClaimsIdentity(claims),
                    Expires = DateTime.UtcNow.AddDays(7), // Use UTC for consistency
                    SigningCredentials = creds,
                    Issuer = _config["JWT:Issuer"],
                    Audience = _config["JWT:Audience"]
                };

                var tokenHandler = new JwtSecurityTokenHandler();
                var token = tokenHandler.CreateToken(tokenDescriptor);

                _logger.LogInformation("JWT token created for user {UserId}", user.Id);
                return tokenHandler.WriteToken(token);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{Method} failed for user {UserId}", nameof(CreateToken), user.Id);
                throw;
            }
        }
    }
}
