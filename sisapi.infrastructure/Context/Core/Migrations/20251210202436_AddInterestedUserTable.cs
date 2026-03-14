using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace sisapi.infrastructure.Context.Core.Migrations
{
    /// <inheritdoc />
    public partial class AddInterestedUserTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "TaxId",
                table: "Companies",
                newName: "Nit");

            migrationBuilder.RenameIndex(
                name: "IX_Companies_TaxId",
                table: "Companies",
                newName: "IX_Companies_Nit");

            migrationBuilder.CreateTable(
                name: "InterestedUsers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Email = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    Password = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    FirstName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    LastName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    PhoneNumber = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    IsConverted = table.Column<bool>(type: "bit", nullable: false),
                    ConvertedToUserId = table.Column<int>(type: "int", nullable: true),
                    ConvertedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedBy = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Active = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InterestedUsers", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_InterestedUsers_Email",
                table: "InterestedUsers",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_InterestedUsers_IsConverted",
                table: "InterestedUsers",
                column: "IsConverted");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "InterestedUsers");

            migrationBuilder.RenameColumn(
                name: "Nit",
                table: "Companies",
                newName: "TaxId");

            migrationBuilder.RenameIndex(
                name: "IX_Companies_Nit",
                table: "Companies",
                newName: "IX_Companies_TaxId");
        }
    }
}
