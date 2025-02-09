using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HNOne.API.Migrations
{
    /// <inheritdoc />
    public partial class Contracts25020701 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "IsCompanyInsurance",
                table: "Contracts",
                newName: "IsCompanyIncomeTax");

            migrationBuilder.RenameColumn(
                name: "IsCompanyInsurance",
                table: "ContractAppendices",
                newName: "IsCompanyIncomeTax");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "IsCompanyIncomeTax",
                table: "Contracts",
                newName: "IsCompanyInsurance");

            migrationBuilder.RenameColumn(
                name: "IsCompanyIncomeTax",
                table: "ContractAppendices",
                newName: "IsCompanyInsurance");
        }
    }
}
