using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace TalentVerse.WebAPI.Migrations
{
    /// <inheritdoc />
    public partial class UpdateEntitiesAccordingToERD : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Convert UserSkills.Type from string to integer
            // First, map string values to integers (assuming "Offer"=0, "Want"=1)
            migrationBuilder.Sql(@"
                ALTER TABLE ""UserSkills"" ADD COLUMN ""TypeTemp"" integer;
                UPDATE ""UserSkills"" SET ""TypeTemp"" = 
                    CASE 
                        WHEN ""Type"" = 'Offer' THEN 0
                        WHEN ""Type"" = 'Want' THEN 1
                        ELSE 0
                    END;
                ALTER TABLE ""UserSkills"" DROP COLUMN ""Type"";
                ALTER TABLE ""UserSkills"" RENAME COLUMN ""TypeTemp"" TO ""Type"";
            ");

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "UserSkills",
                type: "timestamp with time zone",
                nullable: false,
                defaultValueSql: "CURRENT_TIMESTAMP");

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "UserSkills",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "Skills",
                type: "timestamp with time zone",
                nullable: false,
                defaultValueSql: "CURRENT_TIMESTAMP");

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "Skills",
                type: "boolean",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "Reviews",
                type: "timestamp with time zone",
                nullable: false,
                defaultValueSql: "CURRENT_TIMESTAMP");

            // Convert Proposals.Status from string to integer
            // Assuming: Pending=0, Accepted=1, Rejected=2, Completed=3, Cancelled=4
            migrationBuilder.Sql(@"
                ALTER TABLE ""Proposals"" ADD COLUMN ""StatusTemp"" integer;
                UPDATE ""Proposals"" SET ""StatusTemp"" = 
                    CASE 
                        WHEN ""Status"" = 'Pending' THEN 0
                        WHEN ""Status"" = 'Accepted' THEN 1
                        WHEN ""Status"" = 'Rejected' THEN 2
                        WHEN ""Status"" = 'Completed' THEN 3
                        WHEN ""Status"" = 'Cancelled' THEN 4
                        ELSE 0
                    END;
                ALTER TABLE ""Proposals"" DROP COLUMN ""Status"";
                ALTER TABLE ""Proposals"" RENAME COLUMN ""StatusTemp"" TO ""Status"";
            ");

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "Proposals",
                type: "timestamp with time zone",
                nullable: false,
                defaultValueSql: "CURRENT_TIMESTAMP");

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "Proposals",
                type: "timestamp with time zone",
                nullable: false,
                defaultValueSql: "CURRENT_TIMESTAMP");

            migrationBuilder.AddColumn<bool>(
                name: "IsRead",
                table: "Messages",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            // Convert CreditTransactions.Type from string to integer
            // Assuming: Credit=0, Debit=1, Bonus=2, Refund=3
            migrationBuilder.Sql(@"
                ALTER TABLE ""CreditTransactions"" ADD COLUMN ""TypeTemp"" integer;
                UPDATE ""CreditTransactions"" SET ""TypeTemp"" = 
                    CASE 
                        WHEN ""Type"" = 'Credit' THEN 0
                        WHEN ""Type"" = 'Debit' THEN 1
                        WHEN ""Type"" = 'Bonus' THEN 2
                        WHEN ""Type"" = 'Refund' THEN 3
                        ELSE 0
                    END;
                ALTER TABLE ""CreditTransactions"" DROP COLUMN ""Type"";
                ALTER TABLE ""CreditTransactions"" RENAME COLUMN ""TypeTemp"" TO ""Type"";
            ");

            migrationBuilder.AlterColumn<long>(
                name: "TransactionId",
                table: "CreditTransactions",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer")
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn)
                .OldAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "CreditTransactions",
                type: "character varying(255)",
                maxLength: 255,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "AspNetUsers",
                type: "timestamp with time zone",
                nullable: false,
                defaultValueSql: "CURRENT_TIMESTAMP");

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "AspNetUsers",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "AspNetUsers",
                type: "timestamp with time zone",
                nullable: false,
                defaultValueSql: "CURRENT_TIMESTAMP");

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "Appointments",
                type: "timestamp with time zone",
                nullable: false,
                defaultValueSql: "CURRENT_TIMESTAMP");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "UserSkills");

            migrationBuilder.DropColumn(
                name: "Description",
                table: "UserSkills");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "Skills");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "Skills");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "Reviews");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "Proposals");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "Proposals");

            migrationBuilder.DropColumn(
                name: "IsRead",
                table: "Messages");

            migrationBuilder.DropColumn(
                name: "Description",
                table: "CreditTransactions");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "Appointments");

            migrationBuilder.AlterColumn<string>(
                name: "Type",
                table: "UserSkills",
                type: "character varying(10)",
                maxLength: 10,
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "Proposals",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AlterColumn<string>(
                name: "Type",
                table: "CreditTransactions",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AlterColumn<int>(
                name: "TransactionId",
                table: "CreditTransactions",
                type: "integer",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint")
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn)
                .OldAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);
        }
    }
}
