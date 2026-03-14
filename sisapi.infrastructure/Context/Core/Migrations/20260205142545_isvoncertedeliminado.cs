using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace sisapi.infrastructure.Context.Core.Migrations
{
    public partial class isvoncertedeliminado : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_InterestedUsers_IsConverted",
                table: "InterestedUsers");

            migrationBuilder.DropColumn(
                name: "IsConverted",
                table: "InterestedUsers");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsConverted",
                table: "InterestedUsers",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateIndex(
                name: "IX_InterestedUsers_IsConverted",
                table: "InterestedUsers",
                column: "IsConverted");
        }
    }
}
