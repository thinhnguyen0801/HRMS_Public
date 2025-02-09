using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HNOne.API.Migrations
{
    /// <inheritdoc />
    public partial class Payrolls : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Payrolls",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EmployeeId = table.Column<int>(type: "int", nullable: false),
                    EmployeeCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    EmployeeName = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    DepartmentId = table.Column<int>(type: "int", nullable: false),
                    DepartmentCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    DepartmentName = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    BranchId = table.Column<int>(type: "int", nullable: false),
                    Month = table.Column<int>(type: "int", nullable: false),
                    Year = table.Column<int>(type: "int", nullable: false),
                    IsLocked = table.Column<bool>(type: "bit", nullable: false),
                    AttendanceSummaryId = table.Column<int>(type: "int", nullable: false),
                    TNC = table.Column<double>(type: "float", nullable: false),
                    CDM = table.Column<double>(type: "float", nullable: false),
                    GCDM = table.Column<double>(type: "float", nullable: false),
                    CTT = table.Column<double>(type: "float", nullable: false),
                    NL = table.Column<double>(type: "float", nullable: false),
                    NPN = table.Column<double>(type: "float", nullable: false),
                    NCD = table.Column<double>(type: "float", nullable: false),
                    NPKL = table.Column<double>(type: "float", nullable: false),
                    NB = table.Column<double>(type: "float", nullable: false),
                    NKP = table.Column<double>(type: "float", nullable: false),
                    CTPC = table.Column<double>(type: "float", nullable: false),
                    TGDLTVS = table.Column<double>(type: "float", nullable: false),
                    SLDLTVS = table.Column<double>(type: "float", nullable: false),
                    SGT = table.Column<double>(type: "float", nullable: false),
                    SGTC = table.Column<double>(type: "float", nullable: false),
                    GCTC = table.Column<double>(type: "float", nullable: false),
                    TGTC = table.Column<double>(type: "float", nullable: false),
                    SGTCTC = table.Column<double>(type: "float", nullable: false),
                    SGTCTT = table.Column<double>(type: "float", nullable: false),
                    SGTCKT = table.Column<double>(type: "float", nullable: false),
                    ContractId = table.Column<int>(type: "int", nullable: false),
                    ContractCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ContractTypeId = table.Column<int>(type: "int", nullable: false),
                    ContractTypeName = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    ContractAppendixId = table.Column<int>(type: "int", nullable: false),
                    ContractAppendixCode = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    IsCompanyDeduction = table.Column<bool>(type: "bit", nullable: false),
                    IsCompanyIncomeTax = table.Column<bool>(type: "bit", nullable: false),
                    BasicSalary = table.Column<decimal>(type: "decimal(19,6)", nullable: false),
                    NegotiatedSalary = table.Column<decimal>(type: "decimal(19,6)", nullable: false),
                    SalaryCoefficient = table.Column<double>(type: "float", nullable: false),
                    ConvertedNegotiatedSalary = table.Column<decimal>(type: "decimal(19,6)", nullable: false),
                    AllowanceSalary = table.Column<decimal>(type: "decimal(19,6)", nullable: false),
                    TotalSalaryGross = table.Column<decimal>(type: "decimal(19,6)", nullable: false),
                    TotalSalaryNet = table.Column<decimal>(type: "decimal(19,6)", nullable: false),
                    ContributionSalaryOT = table.Column<decimal>(type: "decimal(19,6)", nullable: false),
                    OvertimeSalary = table.Column<decimal>(type: "decimal(19,6)", nullable: false),
                    ActualSalary = table.Column<decimal>(type: "decimal(19,6)", nullable: false),
                    AnnualLeaveSalary = table.Column<decimal>(type: "decimal(19,6)", nullable: false),
                    RegulatedSalary = table.Column<decimal>(type: "decimal(19,6)", nullable: false),
                    HolidaySalary = table.Column<decimal>(type: "decimal(19,6)", nullable: false),
                    MissingWorkingHourSalary = table.Column<decimal>(type: "decimal(19,6)", nullable: false),
                    LateSalary = table.Column<decimal>(type: "decimal(19,6)", nullable: false),
                    DeductionConfigSIId = table.Column<int>(type: "int", nullable: false),
                    CoefficientEnterpriseSI = table.Column<double>(type: "float", nullable: false),
                    CoefficientEmployeeSI = table.Column<double>(type: "float", nullable: false),
                    ContributionSalarySI = table.Column<decimal>(type: "decimal(19,6)", nullable: false),
                    DeductionEnterpriseSI = table.Column<decimal>(type: "decimal(19,6)", nullable: false),
                    DeductionEmployeeSI = table.Column<decimal>(type: "decimal(19,6)", nullable: false),
                    DeductionConfigHIId = table.Column<int>(type: "int", nullable: false),
                    CoefficientEnterpriseHI = table.Column<double>(type: "float", nullable: false),
                    CoefficientEmployeeHI = table.Column<double>(type: "float", nullable: false),
                    ContributionSalaryHI = table.Column<decimal>(type: "decimal(19,6)", nullable: false),
                    DeductionEnterpriseHI = table.Column<decimal>(type: "decimal(19,6)", nullable: false),
                    DeductionEmployeeHI = table.Column<decimal>(type: "decimal(19,6)", nullable: false),
                    DeductionConfigUIId = table.Column<int>(type: "int", nullable: false),
                    CoefficientEnterpriseUI = table.Column<double>(type: "float", nullable: false),
                    CoefficientEmployeeUI = table.Column<double>(type: "float", nullable: false),
                    ContributionSalaryUI = table.Column<decimal>(type: "decimal(19,6)", nullable: false),
                    DeductionEnterpriseUI = table.Column<decimal>(type: "decimal(19,6)", nullable: false),
                    DeductionEmployeeUI = table.Column<decimal>(type: "decimal(19,6)", nullable: false),
                    DeductionConfigAIId = table.Column<int>(type: "int", nullable: false),
                    CoefficientEnterpriseAI = table.Column<double>(type: "float", nullable: false),
                    CoefficientEmployeeAI = table.Column<double>(type: "float", nullable: false),
                    ContributionSalaryAI = table.Column<decimal>(type: "decimal(19,6)", nullable: false),
                    DeductionEnterpriseAI = table.Column<decimal>(type: "decimal(19,6)", nullable: false),
                    DeductionEmployeeAI = table.Column<decimal>(type: "decimal(19,6)", nullable: false),
                    TotalDeductionEnterprise = table.Column<decimal>(type: "decimal(19,6)", nullable: false),
                    TotalDeductionEmployee = table.Column<decimal>(type: "decimal(19,6)", nullable: false),
                    TotalDeduction = table.Column<decimal>(type: "decimal(19,6)", nullable: false),
                    DeductionConfigUFId = table.Column<int>(type: "int", nullable: false),
                    CoefficientEnterpriseUF = table.Column<double>(type: "float", nullable: false),
                    CoefficientEmployeeUF = table.Column<double>(type: "float", nullable: false),
                    UnionFeeSalary = table.Column<decimal>(type: "decimal(19,6)", nullable: false),
                    DeductionEnterpriseUF = table.Column<decimal>(type: "decimal(19,6)", nullable: false),
                    DeductionEmployeeUF = table.Column<decimal>(type: "decimal(19,6)", nullable: false),
                    SalaryParameterId = table.Column<int>(type: "int", nullable: false),
                    TaxTypeCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    TaxTypeName = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    TaxtRateId = table.Column<int>(type: "int", nullable: false),
                    TaxBracket = table.Column<int>(type: "int", nullable: false),
                    MinTaxSalary = table.Column<decimal>(type: "decimal(19,6)", nullable: false),
                    MaxTaxSalary = table.Column<decimal>(type: "decimal(19,6)", nullable: false),
                    TaxRate = table.Column<decimal>(type: "decimal(19,6)", nullable: false),
                    ProgressiveAmount = table.Column<decimal>(type: "decimal(19,6)", nullable: false),
                    StandardTax = table.Column<decimal>(type: "decimal(19,6)", nullable: false),
                    FamilyCircumstanceTaxDeduction = table.Column<decimal>(type: "decimal(19,6)", nullable: false),
                    NumOfPeopleTaxFCTaxDeduction = table.Column<int>(type: "int", nullable: false),
                    TotalFCTaxDeduction = table.Column<decimal>(type: "decimal(19,6)", nullable: false),
                    TaxableIncome = table.Column<decimal>(type: "decimal(19,6)", nullable: false),
                    TaxAllowance = table.Column<decimal>(type: "decimal(19,6)", nullable: false),
                    TaxPayment = table.Column<decimal>(type: "decimal(19,6)", nullable: false),
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
                    table.PrimaryKey("PK_Payrolls", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Payrolls_EmployeeId_BranchId_Month_Year",
                table: "Payrolls",
                columns: new[] { "EmployeeId", "BranchId", "Month", "Year" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Payrolls");
        }
    }
}
