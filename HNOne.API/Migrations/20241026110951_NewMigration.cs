using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HNOne.API.Migrations
{
    /// <inheritdoc />
    public partial class NewMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Branchs",
                columns: table => new
                {
                    BranchId = table.Column<int>(type: "int", nullable: false),
                    BranchCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    BranchName = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    ImgUrl = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Address = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    PhoneNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
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
                    table.PrimaryKey("PK_Branchs", x => x.BranchId);
                });

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

            migrationBuilder.CreateTable(
                name: "Departments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Name = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    ManagerId = table.Column<int>(type: "int", nullable: false),
                    HeadId = table.Column<int>(type: "int", nullable: false),
                    AssistantManagerIds = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Remark = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    BranchId = table.Column<int>(type: "int", nullable: false),
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
                    table.PrimaryKey("PK_Departments", x => x.Id);
                });

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
                    TemporaryAddress = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    ContactAddress = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    Religion = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Ethnicity = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ImageUrl = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    MaritalStatus = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    DateOfJoining = table.Column<DateTime>(type: "datetime2", nullable: true),
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
                    BankBranch = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    Beneficiary = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    Nationality = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    TaxNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    LevelOfEducationId1 = table.Column<int>(type: "int", nullable: true),
                    LevelOfEducationId2 = table.Column<int>(type: "int", nullable: true),
                    MajorId1 = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    MajorId2 = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    EducationalInstitution1 = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    EducationalInstitution2 = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Ranking1 = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    Ranking2 = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    LanguageLevel = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    LevelOfComputerLiteracy = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    OtherSkills = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    ProbationEndDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    BranchId = table.Column<int>(type: "int", nullable: false),
                    DepartmentId = table.Column<int>(type: "int", nullable: false),
                    PositionId = table.Column<int>(type: "int", nullable: false),
                    TitleId = table.Column<int>(type: "int", nullable: true),
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

            migrationBuilder.CreateTable(
                name: "Menus",
                columns: table => new
                {
                    MenuID = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    MenuName = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
                    Icon = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Link = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    Controller = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ParentID = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Level = table.Column<int>(type: "int", nullable: false),
                    IsVisible = table.Column<bool>(type: "bit", nullable: false),
                    OrdinalNumber = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Menus", x => x.MenuID);
                });

            migrationBuilder.CreateTable(
                name: "Positions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Name = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    LevelCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Remark = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    BranchId = table.Column<int>(type: "int", nullable: false),
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
                    table.PrimaryKey("PK_Positions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Titles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Name = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    Remark = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    BranchId = table.Column<int>(type: "int", nullable: false),
                    DepartmentId = table.Column<int>(type: "int", nullable: false),
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
                    table.PrimaryKey("PK_Titles", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Branchs_BranchCode",
                table: "Branchs",
                column: "BranchCode",
                unique: true,
                filter: "[BranchCode] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_ContractTypes_Code",
                table: "ContractTypes",
                column: "Code",
                unique: true,
                filter: "[Code] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Departments_Code",
                table: "Departments",
                column: "Code",
                unique: true,
                filter: "[Code] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Employees_Code",
                table: "Employees",
                column: "Code",
                unique: true,
                filter: "[Code] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Positions_Code",
                table: "Positions",
                column: "Code",
                unique: true,
                filter: "[Code] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Titles_Code",
                table: "Titles",
                column: "Code",
                unique: true,
                filter: "[Code] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Branchs");

            migrationBuilder.DropTable(
                name: "ContractTypes");

            migrationBuilder.DropTable(
                name: "Departments");

            migrationBuilder.DropTable(
                name: "Employees");

            migrationBuilder.DropTable(
                name: "Menus");

            migrationBuilder.DropTable(
                name: "Positions");

            migrationBuilder.DropTable(
                name: "Titles");
        }
    }
}
