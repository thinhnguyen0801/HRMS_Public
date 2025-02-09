using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HNOne.API.Migrations
{
    /// <inheritdoc />
    public partial class AttendanceSummarys20250106 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<double>(
                name: "NL",
                table: "AttendanceSummarys",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "SGTCKT",
                table: "AttendanceSummarys",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "SGTCTC",
                table: "AttendanceSummarys",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "SGTCTT",
                table: "AttendanceSummarys",
                type: "float",
                nullable: false,
                defaultValue: 0.0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "NL",
                table: "AttendanceSummarys");

            migrationBuilder.DropColumn(
                name: "SGTCKT",
                table: "AttendanceSummarys");

            migrationBuilder.DropColumn(
                name: "SGTCTC",
                table: "AttendanceSummarys");

            migrationBuilder.DropColumn(
                name: "SGTCTT",
                table: "AttendanceSummarys");
        }
    }
}
