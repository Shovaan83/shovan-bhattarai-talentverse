using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using TalentVerse.WebAPI.Data;

#nullable disable

namespace TalentVerse.WebAPI.Migrations
{
    /// <inheritdoc />
    [DbContext(typeof(AppDbContext))]
    [Migration("20260607133000_AddUserSkillProficiencyLevel")]
    public partial class AddUserSkillProficiencyLevel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ProficiencyLevel",
                table: "UserSkills",
                type: "integer",
                nullable: false,
                defaultValue: 3);

            migrationBuilder.Sql(@"
                ALTER TABLE ""UserSkills""
                ADD CONSTRAINT ""CK_UserSkills_ProficiencyLevel""
                CHECK (""ProficiencyLevel"" BETWEEN 1 AND 5);
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                ALTER TABLE ""UserSkills""
                DROP CONSTRAINT IF EXISTS ""CK_UserSkills_ProficiencyLevel"";
            ");

            migrationBuilder.DropColumn(
                name: "ProficiencyLevel",
                table: "UserSkills");
        }
    }
}
