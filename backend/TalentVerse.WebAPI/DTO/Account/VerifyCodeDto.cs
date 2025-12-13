using System.ComponentModel.DataAnnotations;

namespace TalentVerse.WebAPI.DTO.Account
{
    public class VerifyCodeDto
    {
        [Required]
        public string Code { get; set; }
    }
}
