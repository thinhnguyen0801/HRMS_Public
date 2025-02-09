using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HNOne.API.Migrations
{
    /// <inheritdoc />
    public partial class PIncomeTaxPeriods140106 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PIncomeTaxPeriods",
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
                    IsCompanyIncomeTax = table.Column<bool>(type: "bit", nullable: false),
                    TaxtRateId = table.Column<int>(type: "int", nullable: false),
                    TaxBracket = table.Column<int>(type: "int", nullable: false),
                    MinTaxSalary = table.Column<decimal>(type: "decimal(19,6)", nullable: false),
                    MaxTaxSalary = table.Column<decimal>(type: "decimal(19,6)", nullable: false),
                    TaxRate = table.Column<double>(type: "float", nullable: false),
                    ProgressiveAmount = table.Column<decimal>(type: "decimal(19,6)", nullable: false),
                    StandardTax = table.Column<decimal>(type: "decimal(19,6)", nullable: false),
                    FamilyCircumstanceTaxDeduction = table.Column<decimal>(type: "decimal(19,6)", nullable: false),
                    NumOfPeopleFCTaxDeduction = table.Column<int>(type: "int", nullable: false),
                    TotalFCTaxDeduction = table.Column<decimal>(type: "decimal(19,6)", nullable: false),
                    TaxableIncome = table.Column<decimal>(type: "decimal(19,6)", nullable: false),
                    TaxAllowance = table.Column<decimal>(type: "decimal(19,6)", nullable: false),
                    TaxPayment = table.Column<decimal>(type: "decimal(19,6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PIncomeTaxPeriods", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PIncomeTaxPeriods_EmployeeId_BranchId_Month_Year",
                table: "PIncomeTaxPeriods",
                columns: new[] { "EmployeeId", "BranchId", "Month", "Year" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PIncomeTaxPeriods");
        }
    }
}
