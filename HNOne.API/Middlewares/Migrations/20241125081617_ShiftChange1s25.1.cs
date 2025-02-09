using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HNOne.API.Migrations
{
    /// <inheritdoc />
    public partial class ShiftChange1s251 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "BgColor",
                table: "ShiftChange1s",
                type: "nvarchar(250)",
                maxLength: 250,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "HolidayId",
                table: "ShiftChange1s",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "IsDayOff",
                table: "ShiftChange1s",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "Symbol",
                table: "ShiftChange1s",
                type: "nvarchar(250)",
                maxLength: 250,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BgColor",
                table: "ShiftChange1s");

            migrationBuilder.DropColumn(
                name: "HolidayId",
                table: "ShiftChange1s");

            migrationBuilder.DropColumn(
                name: "IsDayOff",
                table: "ShiftChange1s");

            migrationBuilder.DropColumn(
                name: "Symbol",
                table: "ShiftChange1s");
        }
    }
}
