using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using TalentVerse.WebAPI.Data;

#nullable disable

namespace TalentVerse.WebAPI.Migrations
{
    /// <inheritdoc />
    [DbContext(typeof(AppDbContext))]
    [Migration("20260608101500_AddDirectionalProposalCredits")]
    public partial class AddDirectionalProposalCredits : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "ProposerCreditAmount",
                table: "Proposals",
                type: "numeric(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "RecipientCreditAmount",
                table: "Proposals",
                type: "numeric(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.Sql(@"
                UPDATE ""Proposals""
                SET ""RecipientCreditAmount"" = ""CreditAmount"",
                    ""ProposerCreditAmount"" = 0
                WHERE ""RecipientCreditAmount"" = 0
                  AND ""ProposerCreditAmount"" = 0
                  AND ""CreditAmount"" > 0");

            migrationBuilder.AddColumn<decimal>(
                name: "ProposerCreditAmount",
                table: "ProposalCounteroffers",
                type: "numeric(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "RecipientCreditAmount",
                table: "ProposalCounteroffers",
                type: "numeric(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.Sql(@"
                UPDATE ""ProposalCounteroffers""
                SET ""RecipientCreditAmount"" = ""CreditAmount"",
                    ""ProposerCreditAmount"" = 0
                WHERE ""RecipientCreditAmount"" = 0
                  AND ""ProposerCreditAmount"" = 0
                  AND ""CreditAmount"" > 0");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ProposerCreditAmount",
                table: "Proposals");

            migrationBuilder.DropColumn(
                name: "RecipientCreditAmount",
                table: "Proposals");

            migrationBuilder.DropColumn(
                name: "ProposerCreditAmount",
                table: "ProposalCounteroffers");

            migrationBuilder.DropColumn(
                name: "RecipientCreditAmount",
                table: "ProposalCounteroffers");
        }
    }
}
