using System.ComponentModel.DataAnnotations;

namespace TalentVerse.WebAPI.DTO.Proposals
{
    public class CreateCounterofferDto
    {
        [Range(typeof(decimal), "0.01", "999999.99", ErrorMessage = "Credit amount must be greater than 0.")]
        public decimal CreditAmount { get; set; }

        [MaxLength(1000, ErrorMessage = "Message cannot exceed 1000 characters")]
        public string? Message { get; set; }
    }
}