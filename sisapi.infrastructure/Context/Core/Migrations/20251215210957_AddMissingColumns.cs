using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace sisapi.infrastructure.Context.Core.Migrations
{
    /// <inheritdoc />
    public partial class AddMissingColumns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CompanyId",
                table: "Roles",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Persons",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FirstName = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    SecondName = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    LastName = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    SurName = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    TypeDocument = table.Column<int>(type: "int", nullable: false),
                    DocumentNumber = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    DocumentException = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    DocumentExpedition = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    PhoneNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Gender = table.Column<int>(type: "int", nullable: false),
                    UserId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Persons", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Persons_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Roles_CompanyId",
                table: "Roles",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_Persons_DocumentNumber",
                table: "Persons",
                column: "DocumentNumber");

            migrationBuilder.CreateIndex(
                name: "IX_Persons_UserId",
                table: "Persons",
                column: "UserId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Roles_Companies_CompanyId",
                table: "Roles",
                column: "CompanyId",
                principalTable: "Companies",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Roles_Companies_CompanyId",
                table: "Roles");

            migrationBuilder.DropTable(
                name: "Persons");

            migrationBuilder.DropIndex(
                name: "IX_Roles_CompanyId",
                table: "Roles");

            migrationBuilder.DropColumn(
                name: "CompanyId",
                table: "Roles");
        }
    }
}
