using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HNOne.API.Migrations
{
    /// <inheritdoc />
    public partial class WorkConfigs : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "WorkConfigs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false),
                    StartDate = table.Column<int>(type: "int", nullable: false),
                    ClosingDate = table.Column<int>(type: "int", nullable: false),
                    ClosingDate1 = table.Column<int>(type: "int", nullable: false),
                    IsLastDayOfMonth = table.Column<bool>(type: "bit", nullable: false),
                    TotalWorkingDayOfMonth = table.Column<double>(type: "float", nullable: false),
                    IsWorkingDayExcludeDayOff = table.Column<bool>(type: "bit", nullable: false),
                    TotalWorkingHours = table.Column<double>(type: "float", nullable: false),
                    SymbolOfWeekdayDayOff = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    BgColorOfWeekdayDayOff = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    SymbolOfHoliday = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    BgColorOfHoliday = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    WorkConfigType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Year = table.Column<int>(type: "int", nullable: false),
                    Month = table.Column<int>(type: "int", nullable: false),
                    TotalWorkingDayOfMonthD = table.Column<double>(type: "float", nullable: false),
                    TotalWorkingHoursD = table.Column<double>(type: "float", nullable: false),
                    StartDateD = table.Column<int>(type: "int", nullable: false),
                    ClosingDateD = table.Column<int>(type: "int", nullable: false),
                    ClosingDate1D = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkConfigs", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "WorkConfigs");
        }
    }
}
