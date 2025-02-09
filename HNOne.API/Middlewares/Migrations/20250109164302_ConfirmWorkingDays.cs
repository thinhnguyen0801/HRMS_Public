using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HNOne.API.Migrations
{
    /// <inheritdoc />
    public partial class ConfirmWorkingDays : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ConfirmWorkingDay1s",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ConfirmWorkingDayId = table.Column<int>(type: "int", nullable: false),
                    EmployeeId = table.Column<int>(type: "int", nullable: false),
                    WorkingDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FromTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ToTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Remark = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    ShiftCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    StartTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    EndTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    StartBreakTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    EndBreakTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    TotalWorkingHours = table.Column<double>(type: "float", nullable: false),
                    StartTimeActual = table.Column<DateTime>(type: "datetime2", nullable: true),
                    EndTimeActual = table.Column<DateTime>(type: "datetime2", nullable: true),
                    StartBreakTimeActual = table.Column<DateTime>(type: "datetime2", nullable: true),
                    EndBreakTimeActual = table.Column<DateTime>(type: "datetime2", nullable: true),
                    TotalWorkingHoursActual = table.Column<double>(type: "float", nullable: false),
                    TotalMissingHours = table.Column<double>(type: "float", nullable: false),
                    DateTracking = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UserSign = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ConfirmWorkingDay1s", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ConfirmWorkingDays",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false),
                    VoucherNo = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    EmployeeId = table.Column<int>(type: "int", nullable: false),
                    WorkingDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EmployeeSignatureId = table.Column<int>(type: "int", nullable: false),
                    DateOfSigning = table.Column<DateTime>(type: "datetime2", nullable: true),
                    BranchId = table.Column<int>(type: "int", nullable: false),
                    DepartmentId = table.Column<int>(type: "int", nullable: false),
                    StatusCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Remark = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    CreateDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UserSign = table.Column<int>(type: "int", nullable: true),
                    UpdateDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UserSign2 = table.Column<int>(type: "int", nullable: true),
                    IsDelete = table.Column<bool>(type: "bit", nullable: false),
                    DeleteReason = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    DateTracking = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ConfirmWorkingDays", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ConfirmWorkingDays_VoucherNo",
                table: "ConfirmWorkingDays",
                column: "VoucherNo",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ConfirmWorkingDay1s");

            migrationBuilder.DropTable(
                name: "ConfirmWorkingDays");
        }
    }
}
