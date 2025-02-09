using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HNOne.API.Migrations
{
    /// <inheritdoc />
    public partial class Famis20250106 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "totalWorkingHoursActual",
                table: "AttendanceSummarys");

            migrationBuilder.AddColumn<DateTime>(
                name: "FromDate",
                table: "FamilyRelationships",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeduction",
                table: "FamilyRelationships",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "ToDate",
                table: "FamilyRelationships",
                type: "datetime2",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FromDate",
                table: "FamilyRelationships");

            migrationBuilder.DropColumn(
                name: "IsDeduction",
                table: "FamilyRelationships");

            migrationBuilder.DropColumn(
                name: "ToDate",
                table: "FamilyRelationships");

            migrationBuilder.AddColumn<double>(
                name: "totalWorkingHoursActual",
                table: "AttendanceSummarys",
                type: "float",
                nullable: false,
                defaultValue: 0.0);
        }
    }
}
