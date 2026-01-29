namespace TalentVerse.WebAPI.Configuration;

public class AppConfigOptions
{
    public int OtpLength { get; set; }
    public int OtpExpiryMinutes { get; set; }
    public int MaxSkillsPerUser { get; set; }
    public int InitialCreditBalance { get; set; }
}