using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HNOne.API.Migrations
{
    /// <inheritdoc />
    public partial class PDeductionPeriods : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PDeductionPeriods",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EmployeeId = table.Column<int>(type: "int", nullable: false),
                    EmployeeCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    BranchId = table.Column<int>(type: "int", nullable: false),
                    Month = table.Column<int>(type: "int", nullable: false),
                    Year = table.Column<int>(type: "int", nullable: false),
                    IsLocked = table.Column<bool>(type: "bit", nullable: false),
                    AttendanceSummaryId = table.Column<int>(type: "int", nullable: false),
                    IsCompanyDeduction = table.Column<bool>(type: "bit", nullable: false),
                    DeductionConfigId = table.Column<int>(type: "int", nullable: false),
                    Type = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ContributionSalary = table.Column<decimal>(type: "decimal(19,6)", nullable: false),
                    CoefficientEnterprise = table.Column<float>(type: "real", nullable: false),
                    CoefficientEmployee = table.Column<float>(type: "real", nullable: false),
                    DeductionEnterprise = table.Column<decimal>(type: "decimal(19,6)", nullable: false),
                    DeductionEmployee = table.Column<decimal>(type: "decimal(19,6)", nullable: false),
                    FromDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ToDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    MaxEnterprise = table.Column<decimal>(type: "decimal(19,6)", nullable: false),
                    MaxEmployee = table.Column<decimal>(type: "decimal(19,6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PDeductionPeriods", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PDeductionPeriods_EmployeeId_BranchId_Month_Year_DeductionConfigId",
                table: "PDeductionPeriods",
                columns: new[] { "EmployeeId", "BranchId", "Month", "Year", "DeductionConfigId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PDeductionPeriods");
        }
    }
}
