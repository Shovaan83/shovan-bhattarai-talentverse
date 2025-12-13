using System.ComponentModel.DataAnnotations;

namespace TalentVerse.WebAPI.DTO.Account
{
    public class UserDto
    {
        public string Username { get; set; }
        public string Email { get; set; }
        public string? Bio { get; set; }
        public string Token { get; set; }
        public string? ProfilePictureUrl { get; set; }
        public bool IsTwoFactorRequired { get; set; } = false;
    }
}
