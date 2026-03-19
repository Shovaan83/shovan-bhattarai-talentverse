namespace TalentVerse.WebAPI.DTO.Credits
{
    public class CreditPackDto
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public int Credits { get; set; }
        public decimal PriceUsd { get; set; }
        public string? BadgeLabel { get; set; } // e.g. "Best Value"
    }
}
