namespace TalentVerse.WebAPI.DTO.Marketplace;

public class UserSearchResultDto
{
    public List<PublicUserDto> Users { get; set; } = new();
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
}
