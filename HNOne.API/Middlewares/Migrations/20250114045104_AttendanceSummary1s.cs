using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HNOne.API.Migrations
{
    /// <inheritdoc />
    public partial class AttendanceSummary1s : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AttendanceSummary1s",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EmployeeId = table.Column<int>(type: "int", nullable: false),
                    EmployeeCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    BranchId = table.Column<int>(type: "int", nullable: false),
                    Month = table.Column<int>(type: "int", nullable: false),
                    Year = table.Column<int>(type: "int", nullable: false),
                    AttendanceSheetCode = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    WorkingDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    StartBreakTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndBreakTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TotalWorkingHours = table.Column<double>(type: "float", nullable: false),
                    TotalWorkingDayOfMonth = table.Column<double>(type: "float", nullable: false),
                    StartDateActual = table.Column<DateTime>(type: "datetime2", nullable: true),
                    EndDateActual = table.Column<DateTime>(type: "datetime2", nullable: true),
                    StartDateConfirmActual = table.Column<DateTime>(type: "datetime2", nullable: true),
                    EndDateConfirmActual = table.Column<DateTime>(type: "datetime2", nullable: true),
                    StartBreakTimeActual = table.Column<DateTime>(type: "datetime2", nullable: true),
                    EndBreakTimeActual = table.Column<DateTime>(type: "datetime2", nullable: true),
                    TotalWorkingHoursActual = table.Column<double>(type: "float", nullable: false),
                    Symbol = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    BgColor = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    IsDayOff = table.Column<double>(type: "float", nullable: false),
                    LeaveConfigId = table.Column<int>(type: "int", nullable: false),
                    HolidayId = table.Column<int>(type: "int", nullable: false),
                    WorkConfigId = table.Column<int>(type: "int", nullable: false),
                    LeaveRequestId = table.Column<int>(type: "int", nullable: false),
                    ReasonId = table.Column<int>(type: "int", nullable: false),
                    OvertimeRequesttId = table.Column<int>(type: "int", nullable: false),
                    LeaveWorkingHourId = table.Column<int>(type: "int", nullable: false),
                    ConfirmWorkingDayId = table.Column<int>(type: "int", nullable: false),
                    Remark = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    CreateDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UserSign = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AttendanceSummary1s", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AttendanceSummary1s_EmployeeId_BranchId_Month_Year",
                table: "AttendanceSummary1s",
                columns: new[] { "EmployeeId", "BranchId", "Month", "Year" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AttendanceSummary1s");
        }
    }
}
