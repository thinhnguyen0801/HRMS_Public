using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HNOne.API.Migrations
{
    /// <inheritdoc />
    public partial class inserttablesalaryconfig : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SalaryConfigurations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false),
                    SalaryCategoryId = table.Column<int>(type: "int", nullable: false),
                    BranchId = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsPersonalIncomeTax = table.Column<bool>(type: "bit", nullable: false),
                    TaxLimit = table.Column<decimal>(type: "decimal(19,6)", nullable: false),
                    IsSocialInsurance = table.Column<bool>(type: "bit", nullable: false),
                    IsHealthInsurance = table.Column<bool>(type: "bit", nullable: false),
                    IsAccidentInsurance = table.Column<bool>(type: "bit", nullable: false),
                    IsOccupationalAccidentInsurance = table.Column<bool>(type: "bit", nullable: false),
                    IsUnionFee = table.Column<bool>(type: "bit", nullable: false),
                    IsOvertime = table.Column<bool>(type: "bit", nullable: false),
                    OvertimeCoefficient = table.Column<double>(type: "float", nullable: false),
                    IsNightShift = table.Column<bool>(type: "bit", nullable: false),
                    CoefficientNightShift = table.Column<double>(type: "float", nullable: false),
                    IsAllowance = table.Column<bool>(type: "bit", nullable: false),
                    IsProbationaryPeriod = table.Column<bool>(type: "bit", nullable: false),
                    SalaryDefault = table.Column<decimal>(type: "decimal(19,6)", nullable: false),
                    SalaryCalculateMethod = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    IsUseOfGradeLevel = table.Column<bool>(type: "bit", nullable: false),
                    CreateDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UserSign = table.Column<int>(type: "int", nullable: true),
                    UpdateDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UserSign2 = table.Column<int>(type: "int", nullable: true),
                    IsDelete = table.Column<bool>(type: "bit", nullable: false),
                    DeleteReason = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DateTracking = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SalaryConfigurations", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SalaryConfigurations_SalaryCategoryId_BranchId",
                table: "SalaryConfigurations",
                columns: new[] { "SalaryCategoryId", "BranchId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SalaryConfigurations");
        }
    }
}
