using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HNOne.API.Migrations
{
    /// <inheritdoc />
    public partial class CheckInOuts : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CheckInOuts",
                columns: table => new
                {
                    Key = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Id = table.Column<int>(type: "int", nullable: false),
                    MaChamCong = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    NgayCham = table.Column<DateTime>(type: "datetime2", nullable: false),
                    GioCham = table.Column<DateTime>(type: "datetime2", nullable: false),
                    KieuCham = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    NguonCham = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    MaSoMay = table.Column<int>(type: "int", nullable: false),
                    TenMay = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CheckInOuts", x => x.Key);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CheckInOuts");
        }
    }
}
