using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebAppExam.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class OutboxMessageIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "idx_outbox_messages_status",
                table: "outbox_messages");

            migrationBuilder.CreateIndex(
                name: "IX_OutboxMessage_Type_Status_IsPermanentFailure",
                table: "outbox_messages",
                columns: new[] { "Type", "Status", "IsPermanentFailure" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_OutboxMessage_Type_Status_IsPermanentFailure",
                table: "outbox_messages");

            migrationBuilder.CreateIndex(
                name: "idx_outbox_messages_status",
                table: "outbox_messages",
                column: "Status");
        }
    }
}
