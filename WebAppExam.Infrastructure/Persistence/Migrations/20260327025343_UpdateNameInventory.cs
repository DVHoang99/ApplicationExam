using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebAppExam.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class UpdateNameInventory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Name",
                table: "inventories",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Name",
                table: "inventories");
        }
    }
}
