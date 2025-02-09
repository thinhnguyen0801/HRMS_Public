using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HNOne.API.Migrations
{
    /// <inheritdoc />
    public partial class AttendanceSummary1s1401 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_AttendanceSummary1s_EmployeeId_BranchId_Month_Year",
                table: "AttendanceSummary1s");

            migrationBuilder.CreateIndex(
                name: "IX_AttendanceSummary1s_EmployeeId_BranchId_Month_Year_WorkingDate",
                table: "AttendanceSummary1s",
                columns: new[] { "EmployeeId", "BranchId", "Month", "Year", "WorkingDate" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_AttendanceSummary1s_EmployeeId_BranchId_Month_Year_WorkingDate",
                table: "AttendanceSummary1s");

            migrationBuilder.CreateIndex(
                name: "IX_AttendanceSummary1s_EmployeeId_BranchId_Month_Year",
                table: "AttendanceSummary1s",
                columns: new[] { "EmployeeId", "BranchId", "Month", "Year" },
                unique: true);
        }
    }
}
