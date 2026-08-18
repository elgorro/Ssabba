using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ssabba.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class MatchAmendWindow : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "AmendWindowMinutes",
                table: "Matches",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "AmendWindowMinutes",
                table: "Communities",
                type: "integer",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AmendWindowMinutes",
                table: "Matches");

            migrationBuilder.DropColumn(
                name: "AmendWindowMinutes",
                table: "Communities");
        }
    }
}
