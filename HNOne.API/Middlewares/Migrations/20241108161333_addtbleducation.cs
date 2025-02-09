using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HNOne.API.Migrations
{
    /// <inheritdoc />
    public partial class addtbleducation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "LevelOfEducations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false),
                    EmployeeId = table.Column<int>(type: "int", nullable: false),
                    FromYear = table.Column<int>(type: "int", nullable: true),
                    ToYear = table.Column<int>(type: "int", nullable: true),
                    LevelOfEducation = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    EducationalInstitution1 = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    EducationalInstitution2 = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    MajorCode = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    RankingCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    RankingName = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    IsComplete = table.Column<bool>(type: "bit", nullable: false),
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
                    table.PrimaryKey("PK_LevelOfEducations", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "LevelOfEducations");
        }
    }
}
