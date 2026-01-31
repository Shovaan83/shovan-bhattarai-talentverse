using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using TalentVerse.WebAPI.Configuration;
using TalentVerse.WebAPI.Interfaces;

namespace TalentVerse.WebAPI.Services;

public class TwoFactorService : ITwoFactorService
{
    private readonly IMemoryCache _cache;
    private readonly AppConfigOptions _config;

    public TwoFactorService(IMemoryCache cache, IOptions<AppConfigOptions> config)
    {
        _cache = cache;
        _config = config?.Value ?? throw new ArgumentNullException(nameof(config));
    }

    public string GenerateCode()
    {
        // Generate code with configured length (e.g., 6 digits = 100000 to 999999)
        var min = (int)Math.Pow(10, _config.OtpLength - 1);
        var max = (int)Math.Pow(10, _config.OtpLength) - 1;
        return Random.Shared.Next(min, max).ToString();
    }

    public Task StoreCodeAsync(string userId, string code)
    {
        // Store code in memory with configured expiry time
        _cache.Set($"2FA_{userId}", code, TimeSpan.FromMinutes(_config.OtpExpiryMinutes));
        return Task.CompletedTask;
    }

    public Task<bool> ValidateCodeAsync(string userId, string code)
    {
        if (_cache.TryGetValue($"2FA_{userId}", out string? storedCode))
        {
            if (storedCode == code)
            {
                _cache.Remove($"2FA_{userId}"); // Invalidate after use
                return Task.FromResult(true);
            }
        }
        return Task.FromResult(false);
    }
}