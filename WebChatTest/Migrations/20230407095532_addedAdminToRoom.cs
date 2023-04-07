using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebChatTest.Migrations
{
    public partial class addedAdminToRoom : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AdminId",
                table: "ChatRooms",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ChatRooms_AdminId",
                table: "ChatRooms",
                column: "AdminId");

            migrationBuilder.AddForeignKey(
                name: "FK_ChatRooms_AspNetUsers_AdminId",
                table: "ChatRooms",
                column: "AdminId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ChatRooms_AspNetUsers_AdminId",
                table: "ChatRooms");

            migrationBuilder.DropIndex(
                name: "IX_ChatRooms_AdminId",
                table: "ChatRooms");

            migrationBuilder.DropColumn(
                name: "AdminId",
                table: "ChatRooms");
        }
    }
}
