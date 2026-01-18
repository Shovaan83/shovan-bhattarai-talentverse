using System.ComponentModel.DataAnnotations;

namespace TalentVerse.WebAPI.DTO.Account;

public class UpdateProfileDto
{
    [MaxLength(256)]
    public string? Username { get; set; }

    [MaxLength(500)]
    public string? Bio { get; set; }

    [MaxLength(2048)]
    public string? ProfilePictureUrl { get; set; }
}
