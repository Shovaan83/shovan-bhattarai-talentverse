namespace TalentVerse.WebAPI.DTO.Proposals
{
    public class ProposalCounterofferDto
    {
        public long ProposalCounterofferId { get; set; }
        public int ProposalId { get; set; }
        public string OfferedByUserId { get; set; } = string.Empty;
        public string OfferedByUsername { get; set; } = string.Empty;
        public decimal CreditAmount { get; set; }
        public decimal ProposerCreditAmount { get; set; }
        public decimal RecipientCreditAmount { get; set; }
        public decimal NetCreditAmount { get; set; }
        public string NetCreditReceiverUserId { get; set; } = string.Empty;
        public string? Message { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
