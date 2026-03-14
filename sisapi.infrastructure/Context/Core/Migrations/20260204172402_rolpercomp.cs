using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace sisapi.infrastructure.Context.Core.Migrations
{
    /// <inheritdoc />
    public partial class rolpercomp : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Add CompanyId column to RolePermissions (nullable for global permissions)
            migrationBuilder.AddColumn<int>(
                name: "CompanyId",
                table: "RolePermissions",
                type: "int",
                nullable: true);

            // Create index for CompanyId
            migrationBuilder.CreateIndex(
                name: "IX_RolePermissions_CompanyId",
                table: "RolePermissions",
                column: "CompanyId");

            // Add foreign key constraint
            migrationBuilder.AddForeignKey(
                name: "FK_RolePermissions_Companies_CompanyId",
                table: "RolePermissions",
                column: "CompanyId",
                principalTable: "Companies",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Drop foreign key
            migrationBuilder.DropForeignKey(
                name: "FK_RolePermissions_Companies_CompanyId",
                table: "RolePermissions");

            // Drop index
            migrationBuilder.DropIndex(
                name: "IX_RolePermissions_CompanyId",
                table: "RolePermissions");

            // Drop column
            migrationBuilder.DropColumn(
                name: "CompanyId",
                table: "RolePermissions");
        }
    }
}
