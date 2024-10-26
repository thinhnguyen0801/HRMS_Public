using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HNOne.API.Migrations
{
    /// <inheritdoc />
    public partial class Emp1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BankCode",
                table: "Employees");

            migrationBuilder.DropColumn(
                name: "PassportNumber",
                table: "Employees");

            migrationBuilder.DropColumn(
                name: "RankingComputer",
                table: "Employees");

            migrationBuilder.DropColumn(
                name: "RankingLang",
                table: "Employees");

            migrationBuilder.AddColumn<string>(
                name: "ContactAddress",
                table: "Employees",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DateOfJoining",
                table: "Employees",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TemporaryAddress",
                table: "Employees",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TitleId",
                table: "Employees",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "ContractTypes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Name = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    Remark = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    BranchId = table.Column<int>(type: "int", nullable: false),
                    StatusCode = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Duration = table.Column<int>(type: "int", nullable: false),
                    IndefiniteDuration = table.Column<int>(type: "int", nullable: false),
                    NumberOfDaysReduced = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
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
                    table.PrimaryKey("PK_ContractTypes", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ContractTypes_Code",
                table: "ContractTypes",
                column: "Code",
                unique: true,
                filter: "[Code] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ContractTypes");

            migrationBuilder.DropColumn(
                name: "ContactAddress",
                table: "Employees");

            migrationBuilder.DropColumn(
                name: "DateOfJoining",
                table: "Employees");

            migrationBuilder.DropColumn(
                name: "TemporaryAddress",
                table: "Employees");

            migrationBuilder.DropColumn(
                name: "TitleId",
                table: "Employees");

            migrationBuilder.AddColumn<string>(
                name: "BankCode",
                table: "Employees",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PassportNumber",
                table: "Employees",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RankingComputer",
                table: "Employees",
                type: "nvarchar(250)",
                maxLength: 250,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RankingLang",
                table: "Employees",
                type: "nvarchar(250)",
                maxLength: 250,
                nullable: true);
        }
    }
}
