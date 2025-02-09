using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HNOne.API.Migrations
{
    /// <inheritdoc />
    public partial class phuluc1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ContractAppendices",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false),
                    ContractId = table.Column<int>(type: "int", nullable: false),
                    EmployeeId = table.Column<int>(type: "int", nullable: false),
                    BranchId = table.Column<int>(type: "int", nullable: false),
                    TimesheetId = table.Column<int>(type: "int", nullable: false),
                    ContractCode = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    ContractAppendixCode = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    DateOfSigning = table.Column<DateTime>(type: "datetime2", nullable: true),
                    EffectiveDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeductionDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    EmployeeSignatureId = table.Column<int>(type: "int", nullable: false),
                    DepartmentId = table.Column<int>(type: "int", nullable: false),
                    PositionId = table.Column<int>(type: "int", nullable: false),
                    TitleId = table.Column<int>(type: "int", nullable: false),
                    PlaceOfWorkId = table.Column<int>(type: "int", nullable: false),
                    ContractNumber = table.Column<int>(type: "int", nullable: false),
                    DecisionNo = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    Remark = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    StatusCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    AuthorizationLetter = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    IsSalaryAdjustment = table.Column<bool>(type: "bit", nullable: false),
                    TaxTypeCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    SalaryCoefficient = table.Column<double>(type: "float", nullable: false),
                    TotalSalary = table.Column<decimal>(type: "decimal(19,6)", nullable: false),
                    NetSalary = table.Column<decimal>(type: "decimal(19,6)", nullable: false),
                    IsCompanyDeduction = table.Column<bool>(type: "bit", nullable: false),
                    IsCompanyInsurance = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ContractAppendices", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ContractAppendices");
        }
    }
}
