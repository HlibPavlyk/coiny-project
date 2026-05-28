using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Coiny.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class SlimSearchOutboxEvent : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EventType",
                table: "SearchOutboxEvents");

            migrationBuilder.DropColumn(
                name: "Payload",
                table: "SearchOutboxEvents");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "EventType",
                table: "SearchOutboxEvents",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Payload",
                table: "SearchOutboxEvents",
                type: "jsonb",
                nullable: false,
                defaultValue: "{}");
        }
    }
}
