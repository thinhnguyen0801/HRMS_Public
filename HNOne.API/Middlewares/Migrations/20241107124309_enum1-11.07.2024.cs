using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HNOne.API.Migrations
{
    /// <inheritdoc />
    public partial class enum111072024 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "CreateDate",
                table: "EnumCatagories",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DeleteReason",
                table: "EnumCatagories",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDelete",
                table: "EnumCatagories",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdateDate",
                table: "EnumCatagories",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "UserSign2",
                table: "EnumCatagories",
                type: "int",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CreateDate",
                table: "EnumCatagories");

            migrationBuilder.DropColumn(
                name: "DeleteReason",
                table: "EnumCatagories");

            migrationBuilder.DropColumn(
                name: "IsDelete",
                table: "EnumCatagories");

            migrationBuilder.DropColumn(
                name: "UpdateDate",
                table: "EnumCatagories");

            migrationBuilder.DropColumn(
                name: "UserSign2",
                table: "EnumCatagories");
        }
    }
}
