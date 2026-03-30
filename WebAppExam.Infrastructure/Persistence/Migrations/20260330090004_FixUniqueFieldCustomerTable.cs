using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebAppExam.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class FixUniqueFieldCustomerTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_customers_Email",
                table: "customers");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_customers_Email",
                table: "customers",
                column: "Email",
                unique: true);
        }
    }
}
