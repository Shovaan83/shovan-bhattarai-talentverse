using TalentVerse.WebAPI.Data.Entities;

namespace TalentVerse.WebAPI.Interfaces
{
    public interface ITokenService
    {
        Task<string> CreateToken(AppUser user);
        
        // ⭐ Refresh token methods
        Task<(string AccessToken, string RefreshToken, DateTime ExpiresAt)> GenerateTokenPairAsync(AppUser user, string ipAddress);
        Task<(string AccessToken, string RefreshToken, DateTime ExpiresAt)?> RefreshTokenAsync(string refreshToken, string ipAddress);
        Task RevokeRefreshTokenAsync(string refreshToken, string ipAddress);
    }
}
