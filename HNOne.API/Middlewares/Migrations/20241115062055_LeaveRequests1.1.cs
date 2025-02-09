using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HNOne.API.Migrations
{
    /// <inheritdoc />
    public partial class LeaveRequests11 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_LeaveRequest1s",
                table: "LeaveRequest1s");

            migrationBuilder.DropColumn(
                name: "Id",
                table: "LeaveRequest1s");

            migrationBuilder.AddColumn<string>(
                name: "Code",
                table: "LeaveRequest1s",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddPrimaryKey(
                name: "PK_LeaveRequest1s",
                table: "LeaveRequest1s",
                column: "Code");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_LeaveRequest1s",
                table: "LeaveRequest1s");

            migrationBuilder.DropColumn(
                name: "Code",
                table: "LeaveRequest1s");

            migrationBuilder.AddColumn<int>(
                name: "Id",
                table: "LeaveRequest1s",
                type: "int",
                nullable: false,
                defaultValue: 0)
                .Annotation("SqlServer:Identity", "1, 1");

            migrationBuilder.AddPrimaryKey(
                name: "PK_LeaveRequest1s",
                table: "LeaveRequest1s",
                column: "Id");
        }
    }
}
