namespace TalentVerse.WebAPI.DTO.Account;

/// <summary>
/// Information about a linked external login provider
/// </summary>
public class LinkedLoginDto
{
    public string Provider { get; set; } = string.Empty;
    public string ProviderDisplayName { get; set; } = string.Empty;
    public string? ProviderKey { get; set; }
}
