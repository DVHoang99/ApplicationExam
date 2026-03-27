using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebAppExam.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class RenameMonthlyRevenuesTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_MonthlyRevenues",
                table: "MonthlyRevenues");

            migrationBuilder.RenameTable(
                name: "MonthlyRevenues",
                newName: "monthly_revenues");

            migrationBuilder.AddPrimaryKey(
                name: "PK_monthly_revenues",
                table: "monthly_revenues",
                column: "MonthYear");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_monthly_revenues",
                table: "monthly_revenues");

            migrationBuilder.RenameTable(
                name: "monthly_revenues",
                newName: "MonthlyRevenues");

            migrationBuilder.AddPrimaryKey(
                name: "PK_MonthlyRevenues",
                table: "MonthlyRevenues",
                column: "MonthYear");
        }
    }
}
