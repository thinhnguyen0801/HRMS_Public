using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HNOne.API.Migrations
{
    /// <inheritdoc />
    public partial class phuluc12 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "CreateDate",
                table: "ContractAppendices",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DateTracking",
                table: "ContractAppendices",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DeleteReason",
                table: "ContractAppendices",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDelete",
                table: "ContractAppendices",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdateDate",
                table: "ContractAppendices",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "UserSign",
                table: "ContractAppendices",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "UserSign2",
                table: "ContractAppendices",
                type: "int",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CreateDate",
                table: "ContractAppendices");

            migrationBuilder.DropColumn(
                name: "DateTracking",
                table: "ContractAppendices");

            migrationBuilder.DropColumn(
                name: "DeleteReason",
                table: "ContractAppendices");

            migrationBuilder.DropColumn(
                name: "IsDelete",
                table: "ContractAppendices");

            migrationBuilder.DropColumn(
                name: "UpdateDate",
                table: "ContractAppendices");

            migrationBuilder.DropColumn(
                name: "UserSign",
                table: "ContractAppendices");

            migrationBuilder.DropColumn(
                name: "UserSign2",
                table: "ContractAppendices");
        }
    }
}
