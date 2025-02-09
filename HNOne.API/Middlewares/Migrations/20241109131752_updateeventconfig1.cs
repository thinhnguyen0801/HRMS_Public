using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HNOne.API.Migrations
{
    /// <inheritdoc />
    public partial class updateeventconfig1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "EventType",
                table: "EventConfigurations",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MenuName",
                table: "EventConfigurations",
                type: "nvarchar(250)",
                maxLength: 250,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ParentID",
                table: "EventConfigurations",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "RowOrder",
                table: "EventConfigurations",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EventType",
                table: "EventConfigurations");

            migrationBuilder.DropColumn(
                name: "MenuName",
                table: "EventConfigurations");

            migrationBuilder.DropColumn(
                name: "ParentID",
                table: "EventConfigurations");

            migrationBuilder.DropColumn(
                name: "RowOrder",
                table: "EventConfigurations");
        }
    }
}
