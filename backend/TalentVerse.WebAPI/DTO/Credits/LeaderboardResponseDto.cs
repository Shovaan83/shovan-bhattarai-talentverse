namespace TalentVerse.WebAPI.DTO.Credits
{
    public class LeaderboardResponseDto
    {
        public IEnumerable<LeaderboardEntryDto> Entries { get; set; } = [];
        public int? CurrentUserRank { get; set; }
        public decimal? CurrentUserBalance { get; set; }
    }
}
