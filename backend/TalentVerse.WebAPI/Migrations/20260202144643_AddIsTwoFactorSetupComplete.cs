using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TalentVerse.WebAPI.Migrations
{
    /// <inheritdoc />
    public partial class AddIsTwoFactorSetupComplete : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsTwoFactorSetupComplete",
                table: "AspNetUsers",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsTwoFactorSetupComplete",
                table: "AspNetUsers");
        }
    }
}
