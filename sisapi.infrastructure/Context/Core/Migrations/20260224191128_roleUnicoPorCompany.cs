using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace sisapi.infrastructure.Context.Core.Migrations
{
    /// <inheritdoc />
    public partial class roleUnicoPorCompany : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Roles_CompanyId",
                table: "Roles");

            migrationBuilder.CreateIndex(
                name: "IX_Roles_CompanyId_Name",
                table: "Roles",
                columns: new[] { "CompanyId", "Name" },
                unique: true,
                filter: "[CompanyId] IS NOT NULL AND [Name] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Roles_CompanyId_Name",
                table: "Roles");

            migrationBuilder.CreateIndex(
                name: "IX_Roles_CompanyId",
                table: "Roles",
                column: "CompanyId");
        }
    }
}
