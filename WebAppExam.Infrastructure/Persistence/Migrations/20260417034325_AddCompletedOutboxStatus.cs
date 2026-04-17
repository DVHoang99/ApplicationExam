using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebAppExam.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddCompletedOutboxStatus : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsPermanentFailure",
                table: "outbox_messages",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "RetryCount",
                table: "outbox_messages",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "idx_outbox_messages_permanent_failure_status",
                table: "outbox_messages",
                columns: new[] { "IsPermanentFailure", "Status" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "idx_outbox_messages_permanent_failure_status",
                table: "outbox_messages");

            migrationBuilder.DropColumn(
                name: "IsPermanentFailure",
                table: "outbox_messages");

            migrationBuilder.DropColumn(
                name: "RetryCount",
                table: "outbox_messages");
        }
    }
}
