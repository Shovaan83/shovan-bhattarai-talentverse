using System.ComponentModel.DataAnnotations;

namespace TalentVerse.WebAPI.DTO.Reviews;

public class CreateReviewDto
{
    [Required(ErrorMessage = "Proposal ID is required")]
    public int ProposalId { get; set; }

    [Required(ErrorMessage = "Rating is required")]
    [Range(1, 5, ErrorMessage = "Rating must be between 1 and 5")]
    public int Rating { get; set; }

    [MaxLength(500, ErrorMessage = "Comment cannot exceed 500 characters")]
    public string? Comment { get; set; }
}
