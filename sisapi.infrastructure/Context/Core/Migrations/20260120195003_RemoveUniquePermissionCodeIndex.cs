using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace sisapi.infrastructure.Context.Core.Migrations
{
    /// <inheritdoc />
    public partial class RemoveUniquePermissionCodeIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Permissions_Code",
                table: "Permissions");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Permissions_Code",
                table: "Permissions",
                column: "Code",
                unique: true);
        }
    }
}
