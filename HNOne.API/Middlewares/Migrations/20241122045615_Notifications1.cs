using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HNOne.API.Migrations
{
    /// <inheritdoc />
    public partial class Notifications1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "VoucherCode",
                table: "Notifications",
                newName: "VoucherNo");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "VoucherNo",
                table: "Notifications",
                newName: "VoucherCode");
        }
    }
}
