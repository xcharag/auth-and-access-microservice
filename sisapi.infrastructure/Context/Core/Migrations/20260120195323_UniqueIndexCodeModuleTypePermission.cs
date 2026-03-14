using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace sisapi.infrastructure.Context.Core.Migrations
{
    public partial class UniqueIndexCodeModuleTypePermission : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Permissions_Code_Module_TypePermission",
                table: "Permissions",
                columns: new[] { "Code", "Module", "TypePermission" },
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Permissions_Code_Module_TypePermission",
                table: "Permissions");
        }
    }
}
