using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace sisapi.infrastructure.Context.Core.Migrations
{
    /// <inheritdoc />
    public partial class RolePermissionPerCompany : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_RolePermissions_CompanyId",
                table: "RolePermissions");

            migrationBuilder.DropIndex(
                name: "IX_RolePermissions_RoleId_PermissionId",
                table: "RolePermissions");

            migrationBuilder.CreateIndex(
                name: "IX_RolePermissions_CompanyId_RoleId_PermissionId",
                table: "RolePermissions",
                columns: new[] { "CompanyId", "RoleId", "PermissionId" },
                unique: true,
                filter: "[CompanyId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_RolePermissions_RoleId",
                table: "RolePermissions",
                column: "RoleId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_RolePermissions_CompanyId_RoleId_PermissionId",
                table: "RolePermissions");

            migrationBuilder.DropIndex(
                name: "IX_RolePermissions_RoleId",
                table: "RolePermissions");

            migrationBuilder.CreateIndex(
                name: "IX_RolePermissions_CompanyId",
                table: "RolePermissions",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_RolePermissions_RoleId_PermissionId",
                table: "RolePermissions",
                columns: new[] { "RoleId", "PermissionId" },
                unique: true);
        }
    }
}
