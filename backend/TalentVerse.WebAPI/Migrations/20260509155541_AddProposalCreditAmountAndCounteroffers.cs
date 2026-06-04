using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace TalentVerse.WebAPI.Migrations
{
    /// <inheritdoc />
    public partial class AddProposalCreditAmountAndCounteroffers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "CreditAmount",
                table: "Proposals",
                type: "numeric(18,2)",
                nullable: false,
                defaultValue: 10m);

            migrationBuilder.Sql(@"UPDATE ""Proposals"" SET ""CreditAmount"" = 10 WHERE ""CreditAmount"" = 0");

            migrationBuilder.CreateTable(
                name: "ProposalCounteroffers",
                columns: table => new
                {
                    ProposalCounterofferId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ProposalId = table.Column<int>(type: "integer", nullable: false),
                    OfferedByUserId = table.Column<string>(type: "text", nullable: false),
                    CreditAmount = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    Message = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProposalCounteroffers", x => x.ProposalCounterofferId);
                    table.ForeignKey(
                        name: "FK_ProposalCounteroffers_AspNetUsers_OfferedByUserId",
                        column: x => x.OfferedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ProposalCounteroffers_Proposals_ProposalId",
                        column: x => x.ProposalId,
                        principalTable: "Proposals",
                        principalColumn: "ProposalId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ProposalCounteroffers_OfferedByUserId",
                table: "ProposalCounteroffers",
                column: "OfferedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_ProposalCounteroffers_ProposalId",
                table: "ProposalCounteroffers",
                column: "ProposalId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ProposalCounteroffers");

            migrationBuilder.DropColumn(
                name: "CreditAmount",
                table: "Proposals");
        }
    }
}
