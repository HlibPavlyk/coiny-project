using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Coiny.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class RenameOutboxEventToSearchOutboxEvent : Migration
    {
        // Rename in place (ALTER TABLE ... RENAME) instead of EF's scaffolded drop+create, which
        // would discard existing rows. The table, its filtered index, and the PK constraint are all
        // renamed so the database stays consistent with the model snapshot.
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameTable(
                name: "OutboxEvents",
                newName: "SearchOutboxEvents");

            migrationBuilder.RenameIndex(
                name: "IX_OutboxEvents_Id",
                table: "SearchOutboxEvents",
                newName: "IX_SearchOutboxEvents_Id");

            migrationBuilder.Sql(
                "ALTER TABLE \"SearchOutboxEvents\" RENAME CONSTRAINT \"PK_OutboxEvents\" TO \"PK_SearchOutboxEvents\";");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                "ALTER TABLE \"SearchOutboxEvents\" RENAME CONSTRAINT \"PK_SearchOutboxEvents\" TO \"PK_OutboxEvents\";");

            migrationBuilder.RenameIndex(
                name: "IX_SearchOutboxEvents_Id",
                table: "SearchOutboxEvents",
                newName: "IX_OutboxEvents_Id");

            migrationBuilder.RenameTable(
                name: "SearchOutboxEvents",
                newName: "OutboxEvents");
        }
    }
}
