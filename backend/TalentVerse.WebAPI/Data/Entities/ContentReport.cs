using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TalentVerse.WebAPI.Data.Entities;

public class ContentReport
{
    [Key]
    public int Id { get; set; }

    [Required]
    public string ReporterId { get; set; } = string.Empty;
    [ForeignKey("ReporterId")]
    public virtual AppUser Reporter { get; set; } = null!;

    [Required]
    [MaxLength(20)]
    public string ContentType { get; set; } = string.Empty; // "Skill" or "Review"

    [Required]
    public int ContentId { get; set; }

    [Required]
    [MaxLength(500)]
    public string Reason { get; set; } = string.Empty;

    [MaxLength(20)]
    public string Status { get; set; } = "Pending"; // "Pending", "Resolved", "Dismissed"

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? ResolvedAt { get; set; }

    public string? ResolvedByAdminId { get; set; }
    [ForeignKey("ResolvedByAdminId")]
    public virtual AppUser? ResolvedByAdmin { get; set; }
}
