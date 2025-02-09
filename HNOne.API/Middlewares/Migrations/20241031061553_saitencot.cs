using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HNOne.API.Migrations
{
    /// <inheritdoc />
    public partial class saitencot : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TaxtTypeCode",
                table: "Contracts");

            migrationBuilder.AddColumn<string>(
                name: "TaxTypeCode",
                table: "Contracts",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TaxTypeCode",
                table: "Contracts");

            migrationBuilder.AddColumn<string>(
                name: "TaxtTypeCode",
                table: "Contracts",
                type: "nvarchar(max)",
                nullable: true);
        }
    }
}
