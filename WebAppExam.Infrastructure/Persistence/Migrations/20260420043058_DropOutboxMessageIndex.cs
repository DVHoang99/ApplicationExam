using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebAppExam.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class DropOutboxMessageIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "idx_outbox_messages_created_at",
                table: "outbox_messages");

            migrationBuilder.DropIndex(
                name: "idx_outbox_messages_permanent_failure_status",
                table: "outbox_messages");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "idx_outbox_messages_created_at",
                table: "outbox_messages",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "idx_outbox_messages_permanent_failure_status",
                table: "outbox_messages",
                columns: new[] { "IsPermanentFailure", "Status" });
        }
    }
}
