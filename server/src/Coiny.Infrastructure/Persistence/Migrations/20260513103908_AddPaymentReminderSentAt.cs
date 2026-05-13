using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Coiny.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddPaymentReminderSentAt : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "ReminderSentAt",
                table: "Payments",
                type: "timestamptz",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ReminderSentAt",
                table: "Payments");
        }
    }
}
