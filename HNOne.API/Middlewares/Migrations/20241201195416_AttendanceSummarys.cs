using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HNOne.API.Migrations
{
    /// <inheritdoc />
    public partial class AttendanceSummarys : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AttendanceSummarys",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EmployeeId = table.Column<int>(type: "int", nullable: false),
                    EmployeeCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    BranchId = table.Column<int>(type: "int", nullable: false),
                    DepartmentId = table.Column<int>(type: "int", nullable: false),
                    PositionId = table.Column<int>(type: "int", nullable: false),
                    TitleId = table.Column<int>(type: "int", nullable: false),
                    ShiftCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Month = table.Column<int>(type: "int", nullable: false),
                    Year = table.Column<int>(type: "int", nullable: false),
                    totalWorkingHoursActual = table.Column<double>(type: "float", nullable: false),
                    TNC = table.Column<double>(type: "float", nullable: false),
                    CDM = table.Column<double>(type: "float", nullable: false),
                    CTT = table.Column<double>(type: "float", nullable: false),
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
                    IsLocked = table.Column<bool>(type: "bit", nullable: false),
                    N01 = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    N02 = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    N03 = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    N04 = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    N05 = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    N06 = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    N07 = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    N08 = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    N09 = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    N10 = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    N11 = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    N12 = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    N13 = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    N14 = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    N15 = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    N16 = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    N17 = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    N18 = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    N19 = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    N20 = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    N21 = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    N22 = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    N23 = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    N24 = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    N25 = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    N26 = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    N27 = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    N28 = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    N29 = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    N30 = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    N31 = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
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
                    table.PrimaryKey("PK_AttendanceSummarys", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AttendanceSummarys_EmployeeId_BranchId_Month_Year",
                table: "AttendanceSummarys",
                columns: new[] { "EmployeeId", "BranchId", "Month", "Year" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AttendanceSummarys");
        }
    }
}
