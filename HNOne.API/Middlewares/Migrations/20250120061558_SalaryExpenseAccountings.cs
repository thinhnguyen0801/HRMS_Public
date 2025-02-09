using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HNOne.API.Migrations
{
    /// <inheritdoc />
    public partial class SalaryExpenseAccountings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SalaryExpenseAccounting1s",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SalaryExpenseAccountingId = table.Column<int>(type: "int", nullable: false),
                    LineId = table.Column<int>(type: "int", nullable: false),
                    SalaryCatagoryCode = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    SalaryCatagoryName = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    Account1 = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Account2 = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    LineTotal = table.Column<decimal>(type: "decimal(19,6)", nullable: false),
                    DateTracking = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UserSign = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SalaryExpenseAccounting1s", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SalaryExpenseAccountings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false),
                    VoucherNo = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    StatusCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    EmployeeSignatureId = table.Column<int>(type: "int", nullable: false),
                    DateOfSigning = table.Column<DateTime>(type: "datetime2", nullable: true),
                    BranchId = table.Column<int>(type: "int", nullable: false),
                    Month = table.Column<int>(type: "int", nullable: false),
                    Year = table.Column<int>(type: "int", nullable: false),
                    DocDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DueDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Remark = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    DocTotal = table.Column<decimal>(type: "decimal(19,6)", nullable: false),
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
                    table.PrimaryKey("PK_SalaryExpenseAccountings", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SalaryExpenseAccountings_VoucherNo",
                table: "SalaryExpenseAccountings",
                column: "VoucherNo",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SalaryExpenseAccounting1s");

            migrationBuilder.DropTable(
                name: "SalaryExpenseAccountings");
        }
    }
}
