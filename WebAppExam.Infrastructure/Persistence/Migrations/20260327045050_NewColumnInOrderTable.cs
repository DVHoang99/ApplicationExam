using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebAppExam.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class NewColumnInOrderTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Address",
                table: "orders",
                type: "character varying(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "CustomerName",
                table: "orders",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "PhoneNumber",
                table: "orders",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Address",
                table: "orders");

            migrationBuilder.DropColumn(
                name: "CustomerName",
                table: "orders");

            migrationBuilder.DropColumn(
                name: "PhoneNumber",
                table: "orders");
        }
    }
}
