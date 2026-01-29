namespace TalentVerse.WebAPI.Configuration;

public class RateLimitingOptions
{
    public int PermitLimit { get; set; } = 50;
    public int WindowMinutes { get; set; } = 5;
}
