using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OBD.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class a7 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Messages_Admins_AdminId",
                table: "Messages");

            migrationBuilder.DropIndex(
                name: "IX_Messages_AdminId",
                table: "Messages");

            migrationBuilder.DropColumn(
                name: "AdminId",
                table: "Messages");

            migrationBuilder.DropColumn(
                name: "OpenRate",
                table: "Messages");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "AdminId",
                table: "Messages",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "OpenRate",
                table: "Messages",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_Messages_AdminId",
                table: "Messages",
                column: "AdminId");

            migrationBuilder.AddForeignKey(
                name: "FK_Messages_Admins_AdminId",
                table: "Messages",
                column: "AdminId",
                principalTable: "Admins",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
