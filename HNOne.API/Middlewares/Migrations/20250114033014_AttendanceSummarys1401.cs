using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HNOne.API.Migrations
{
    /// <inheritdoc />
    public partial class AttendanceSummarys1401 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "N32",
                table: "AttendanceSummarys",
                newName: "TitleCode");

            migrationBuilder.AddColumn<string>(
                name: "DepartmentCode",
                table: "AttendanceSummarys",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DepartmentName",
                table: "AttendanceSummarys",
                type: "nvarchar(250)",
                maxLength: 250,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EmployeeName",
                table: "AttendanceSummarys",
                type: "nvarchar(250)",
                maxLength: 250,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PositionCode",
                table: "AttendanceSummarys",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PositionName",
                table: "AttendanceSummarys",
                type: "nvarchar(250)",
                maxLength: 250,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TitleName",
                table: "AttendanceSummarys",
                type: "nvarchar(250)",
                maxLength: 250,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DepartmentCode",
                table: "AttendanceSummarys");

            migrationBuilder.DropColumn(
                name: "DepartmentName",
                table: "AttendanceSummarys");

            migrationBuilder.DropColumn(
                name: "EmployeeName",
                table: "AttendanceSummarys");

            migrationBuilder.DropColumn(
                name: "PositionCode",
                table: "AttendanceSummarys");

            migrationBuilder.DropColumn(
                name: "PositionName",
                table: "AttendanceSummarys");

            migrationBuilder.DropColumn(
                name: "TitleName",
                table: "AttendanceSummarys");

            migrationBuilder.RenameColumn(
                name: "TitleCode",
                table: "AttendanceSummarys",
                newName: "N32");
        }
    }
}
