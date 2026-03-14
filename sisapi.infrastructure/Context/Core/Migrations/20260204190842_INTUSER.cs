using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace sisapi.infrastructure.Context.Core.Migrations
{
    /// <inheritdoc />
    public partial class INTUSER : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsAccepted",
                table: "InterestedUsers",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsAccepted",
                table: "InterestedUsers");
        }
    }
}
