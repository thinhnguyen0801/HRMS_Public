using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HNOne.API.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
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

            migrationBuilder.CreateIndex(
                name: "IX_Branchs_BranchCode",
                table: "Branchs",
                column: "BranchCode",
                unique: true,
                filter: "[BranchCode] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Branchs");

            migrationBuilder.DropTable(
                name: "Menus");
        }
    }
}
