using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HNOne.API.Migrations
{
    /// <inheritdoc />
    public partial class Contracts250207 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TimesheetId",
                table: "ContractAppendices");

            migrationBuilder.RenameColumn(
                name: "TimesheetId",
                table: "Contracts",
                newName: "DepartmentId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "DepartmentId",
                table: "Contracts",
                newName: "TimesheetId");

            migrationBuilder.AddColumn<int>(
                name: "TimesheetId",
                table: "ContractAppendices",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }
    }
}
