// backend/TalentVerse.WebAPI/Data/Entities/CreditTransaction.cs
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TalentVerse.WebAPI.Data.Enums;

namespace TalentVerse.WebAPI.Data.Entities
{
    public class CreditTransaction
    {
        [Key]
        public long TransactionId { get; set; }

        [Required]
        public string UserId { get; set; } 
        [ForeignKey("UserId")]
        public virtual AppUser User { get; set; }

        [Required]
        public TransactionType Type { get; set; }

        [Required]
        [Column(TypeName = "decimal(18, 2)")] 
        public decimal Amount { get; set; }

        public DateTime TransactionDate { get; set; } = DateTime.UtcNow;

        [MaxLength(255)]
        public string? Description { get; set; }
    }
}