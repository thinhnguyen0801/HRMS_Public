using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HNOne.API.Migrations
{
    /// <inheritdoc />
    public partial class LeaveRequest1s251 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "BgColor",
                table: "OvertimeRequest1s",
                type: "nvarchar(250)",
                maxLength: 250,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDayOff",
                table: "OvertimeRequest1s",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "Symbol",
                table: "OvertimeRequest1s",
                type: "nvarchar(250)",
                maxLength: 250,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BgColor",
                table: "LeaveRequest1s",
                type: "nvarchar(250)",
                maxLength: 250,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDayOff",
                table: "LeaveRequest1s",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "Symbol",
                table: "LeaveRequest1s",
                type: "nvarchar(250)",
                maxLength: 250,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BgColor",
                table: "OvertimeRequest1s");

            migrationBuilder.DropColumn(
                name: "IsDayOff",
                table: "OvertimeRequest1s");

            migrationBuilder.DropColumn(
                name: "Symbol",
                table: "OvertimeRequest1s");

            migrationBuilder.DropColumn(
                name: "BgColor",
                table: "LeaveRequest1s");

            migrationBuilder.DropColumn(
                name: "IsDayOff",
                table: "LeaveRequest1s");

            migrationBuilder.DropColumn(
                name: "Symbol",
                table: "LeaveRequest1s");
        }
    }
}
