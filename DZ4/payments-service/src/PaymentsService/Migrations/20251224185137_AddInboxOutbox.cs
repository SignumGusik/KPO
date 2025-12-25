using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PaymentsService.Migrations
{
    /// <inheritdoc />
    public partial class AddInboxOutbox : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ledger_user_id",
                table: "ledger");

            migrationBuilder.CreateTable(
                name: "inbox",
                columns: table => new
                {
                    event_id = table.Column<Guid>(type: "uuid", nullable: false),
                    received_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_inbox", x => x.event_id);
                });

            migrationBuilder.CreateTable(
                name: "outbox",
                columns: table => new
                {
                    event_id = table.Column<Guid>(type: "uuid", nullable: false),
                    event_type = table.Column<string>(type: "text", nullable: false),
                    payload_json = table.Column<string>(type: "jsonb", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    published_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    publish_attempts = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_outbox", x => x.event_id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ledger_order_id_type",
                table: "ledger",
                columns: new[] { "order_id", "type" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_outbox_published_at",
                table: "outbox",
                column: "published_at");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "inbox");

            migrationBuilder.DropTable(
                name: "outbox");

            migrationBuilder.DropIndex(
                name: "IX_ledger_order_id_type",
                table: "ledger");

            migrationBuilder.CreateIndex(
                name: "IX_ledger_user_id",
                table: "ledger",
                column: "user_id");
        }
    }
}
