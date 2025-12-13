namespace TalentVerse.WebAPI.Services
{
    public interface ITwoFactorService
    {
        string GenerateCode();
        Task<bool> StoreCodeAsync(string userId, string code);
        Task<bool> ValidateCodeAsync(string userId, string code);
    }

    public class TwoFactorService : ITwoFactorService
    {
        private static readonly Dictionary<string, (string Code, DateTime Expiry)> _codes = new();
        private const int CodeExpiryMinutes = 10;

        public string GenerateCode()
        {
            return Random.Shared.Next(100000, 999999).ToString();
        }

        public Task<bool> StoreCodeAsync(string userId, string code)
        {
            _codes[userId] = (code, DateTime.UtcNow.AddMinutes(CodeExpiryMinutes));
            return Task.FromResult(true);
        }

        public Task<bool> ValidateCodeAsync(string userId, string code)
        {
            if (_codes.TryGetValue(userId, out var stored))
            {
                if (stored.Expiry > DateTime.UtcNow && stored.Code == code)
                {
                    _codes.Remove(userId); // Remove after successful validation
                    return Task.FromResult(true);
                }
                
                // Remove expired codes
                if (stored.Expiry <= DateTime.UtcNow)
                {
                    _codes.Remove(userId);
                }
            }

            return Task.FromResult(false);
        }
    }
}
