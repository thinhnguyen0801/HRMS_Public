using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HNOne.API.Migrations
{
    /// <inheritdoc />
    public partial class AttendanceSummary1s140102 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "DepartmentCode",
                table: "AttendanceSummary1s",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "DepartmentId",
                table: "AttendanceSummary1s",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "DepartmentName",
                table: "AttendanceSummary1s",
                type: "nvarchar(250)",
                maxLength: 250,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "DocEntry",
                table: "AttendanceSummary1s",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "EmployeeName",
                table: "AttendanceSummary1s",
                type: "nvarchar(250)",
                maxLength: 250,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ObjType",
                table: "AttendanceSummary1s",
                type: "nvarchar(250)",
                maxLength: 250,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PositionCode",
                table: "AttendanceSummary1s",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PositionId",
                table: "AttendanceSummary1s",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "PositionName",
                table: "AttendanceSummary1s",
                type: "nvarchar(250)",
                maxLength: 250,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ShiftCode",
                table: "AttendanceSummary1s",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "TitleCode",
                table: "AttendanceSummary1s",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TitleId",
                table: "AttendanceSummary1s",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "TitleName",
                table: "AttendanceSummary1s",
                type: "nvarchar(250)",
                maxLength: 250,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "VoucherNo",
                table: "AttendanceSummary1s",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DepartmentCode",
                table: "AttendanceSummary1s");

            migrationBuilder.DropColumn(
                name: "DepartmentId",
                table: "AttendanceSummary1s");

            migrationBuilder.DropColumn(
                name: "DepartmentName",
                table: "AttendanceSummary1s");

            migrationBuilder.DropColumn(
                name: "DocEntry",
                table: "AttendanceSummary1s");

            migrationBuilder.DropColumn(
                name: "EmployeeName",
                table: "AttendanceSummary1s");

            migrationBuilder.DropColumn(
                name: "ObjType",
                table: "AttendanceSummary1s");

            migrationBuilder.DropColumn(
                name: "PositionCode",
                table: "AttendanceSummary1s");

            migrationBuilder.DropColumn(
                name: "PositionId",
                table: "AttendanceSummary1s");

            migrationBuilder.DropColumn(
                name: "PositionName",
                table: "AttendanceSummary1s");

            migrationBuilder.DropColumn(
                name: "ShiftCode",
                table: "AttendanceSummary1s");

            migrationBuilder.DropColumn(
                name: "TitleCode",
                table: "AttendanceSummary1s");

            migrationBuilder.DropColumn(
                name: "TitleId",
                table: "AttendanceSummary1s");

            migrationBuilder.DropColumn(
                name: "TitleName",
                table: "AttendanceSummary1s");

            migrationBuilder.DropColumn(
                name: "VoucherNo",
                table: "AttendanceSummary1s");
        }
    }
}
