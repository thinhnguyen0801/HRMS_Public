using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HNOne.API.Migrations
{
    /// <inheritdoc />
    public partial class WorkConfigs1010 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "BgColorOfLeaveOfAbsence",
                table: "WorkConfigs",
                type: "nvarchar(250)",
                maxLength: 250,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BgColorOfOvertime",
                table: "WorkConfigs",
                type: "nvarchar(250)",
                maxLength: 250,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BgColorOfUnpaidLeave",
                table: "WorkConfigs",
                type: "nvarchar(250)",
                maxLength: 250,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SymbolOfLeaveOfAbsence",
                table: "WorkConfigs",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "SymbolOfOvertime",
                table: "WorkConfigs",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "SymbolOfUnpaidLeave",
                table: "WorkConfigs",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BgColorOfLeaveOfAbsence",
                table: "WorkConfigs");

            migrationBuilder.DropColumn(
                name: "BgColorOfOvertime",
                table: "WorkConfigs");

            migrationBuilder.DropColumn(
                name: "BgColorOfUnpaidLeave",
                table: "WorkConfigs");

            migrationBuilder.DropColumn(
                name: "SymbolOfLeaveOfAbsence",
                table: "WorkConfigs");

            migrationBuilder.DropColumn(
                name: "SymbolOfOvertime",
                table: "WorkConfigs");

            migrationBuilder.DropColumn(
                name: "SymbolOfUnpaidLeave",
                table: "WorkConfigs");
        }
    }
}
