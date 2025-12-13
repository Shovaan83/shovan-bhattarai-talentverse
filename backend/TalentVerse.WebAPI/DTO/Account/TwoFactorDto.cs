namespace TalentVerse.WebAPI.DTO.Account
{
    public class TwoFactorDto
    {
        public string Key { get; set; }
        public string AuthenticatorUri { get; set; }
    }
}
