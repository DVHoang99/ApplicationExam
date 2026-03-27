using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebAppExam.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class UpdateIndexInventory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_inventories_ProductId",
                table: "inventories");

            migrationBuilder.CreateIndex(
                name: "IX_inventories_ProductId",
                table: "inventories",
                column: "ProductId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_inventories_ProductId",
                table: "inventories");

            migrationBuilder.CreateIndex(
                name: "IX_inventories_ProductId",
                table: "inventories",
                column: "ProductId",
                unique: true);
        }
    }
}
