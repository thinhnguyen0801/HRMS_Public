using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HNOne.API.Migrations
{
    /// <inheritdoc />
    public partial class Emp : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Employees",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Name = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    DateOfBirth = table.Column<DateTime>(type: "datetime2", nullable: true),
                    StatusId = table.Column<int>(type: "int", nullable: true),
                    Gender = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    PlaceOfBirth = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    PlaceOfOrigin = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    Religion = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Ethnicity = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ImageUrl = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    MaritalStatus = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Remark = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    CIC = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    IssuanceDateCIC = table.Column<DateTime>(type: "datetime2", nullable: true),
                    PlaceOfIssuanceCIC = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    Phone1 = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Phone2 = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Phone3 = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Email1 = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    Email2 = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    AccountNumber = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    BankName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    BankCode = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    BankBranch = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    Beneficiary = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    Nationality = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    TaxNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    PassportNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    LevelOfEducationId1 = table.Column<int>(type: "int", nullable: true),
                    LevelOfEducationId2 = table.Column<int>(type: "int", nullable: true),
                    MajorId1 = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    MajorId2 = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    EducationalInstitution1 = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    EducationalInstitution2 = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Ranking1 = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    Ranking2 = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    LanguageLevel = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    RankingLang = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    LevelOfComputerLiteracy = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    RankingComputer = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    OtherSkills = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    ProbationEndDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    BranchId = table.Column<int>(type: "int", nullable: false),
                    DepartmentId = table.Column<int>(type: "int", nullable: false),
                    PositionId = table.Column<int>(type: "int", nullable: false),
                    ManagerId = table.Column<int>(type: "int", nullable: true),
                    AttendanceSheetId = table.Column<int>(type: "int", nullable: true),
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
                    table.PrimaryKey("PK_Employees", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Employees_Code",
                table: "Employees",
                column: "Code",
                unique: true,
                filter: "[Code] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Employees");
        }
    }
}
