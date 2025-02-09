using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HNOne.API.Migrations
{
    /// <inheritdoc />
    public partial class HolidayCatagories1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "CreateDate",
                table: "HolidayCatagories",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DateTracking",
                table: "HolidayCatagories",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DeleteReason",
                table: "HolidayCatagories",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDelete",
                table: "HolidayCatagories",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdateDate",
                table: "HolidayCatagories",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "UserSign",
                table: "HolidayCatagories",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "UserSign2",
                table: "HolidayCatagories",
                type: "int",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CreateDate",
                table: "HolidayCatagories");

            migrationBuilder.DropColumn(
                name: "DateTracking",
                table: "HolidayCatagories");

            migrationBuilder.DropColumn(
                name: "DeleteReason",
                table: "HolidayCatagories");

            migrationBuilder.DropColumn(
                name: "IsDelete",
                table: "HolidayCatagories");

            migrationBuilder.DropColumn(
                name: "UpdateDate",
                table: "HolidayCatagories");

            migrationBuilder.DropColumn(
                name: "UserSign",
                table: "HolidayCatagories");

            migrationBuilder.DropColumn(
                name: "UserSign2",
                table: "HolidayCatagories");
        }
    }
}
