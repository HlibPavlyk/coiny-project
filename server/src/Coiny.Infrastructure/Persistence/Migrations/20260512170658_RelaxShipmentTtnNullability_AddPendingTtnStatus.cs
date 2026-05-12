using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Coiny.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class RelaxShipmentTtnNullability_AddPendingTtnStatus : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Shipments_NovaPoshtaTtn",
                table: "Shipments");

            migrationBuilder.DropIndex(
                name: "IX_Shipments_PaymentId",
                table: "Shipments");

            migrationBuilder.DropIndex(
                name: "IX_Shipments_Status",
                table: "Shipments");

            migrationBuilder.AlterColumn<Guid>(
                name: "PaymentId",
                table: "Shipments",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AlterColumn<string>(
                name: "NovaPoshtaTtn",
                table: "Shipments",
                type: "character varying(32)",
                maxLength: 32,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(32)",
                oldMaxLength: 32);

            migrationBuilder.AlterColumn<string>(
                name: "IntDocNumber",
                table: "Shipments",
                type: "character varying(32)",
                maxLength: 32,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(32)",
                oldMaxLength: 32);

            migrationBuilder.CreateIndex(
                name: "IX_Shipments_NovaPoshtaTtn",
                table: "Shipments",
                column: "NovaPoshtaTtn",
                unique: true,
                filter: "\"NovaPoshtaTtn\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Shipments_PaymentId",
                table: "Shipments",
                column: "PaymentId",
                unique: true,
                filter: "\"PaymentId\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Shipments_Status",
                table: "Shipments",
                column: "Status",
                filter: "\"Status\" NOT IN ('PendingTtn','Delivered','Refused','Returned','Lost')");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Shipments_NovaPoshtaTtn",
                table: "Shipments");

            migrationBuilder.DropIndex(
                name: "IX_Shipments_PaymentId",
                table: "Shipments");

            migrationBuilder.DropIndex(
                name: "IX_Shipments_Status",
                table: "Shipments");

            migrationBuilder.AlterColumn<Guid>(
                name: "PaymentId",
                table: "Shipments",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "NovaPoshtaTtn",
                table: "Shipments",
                type: "character varying(32)",
                maxLength: 32,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "character varying(32)",
                oldMaxLength: 32,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "IntDocNumber",
                table: "Shipments",
                type: "character varying(32)",
                maxLength: 32,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "character varying(32)",
                oldMaxLength: 32,
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Shipments_NovaPoshtaTtn",
                table: "Shipments",
                column: "NovaPoshtaTtn",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Shipments_PaymentId",
                table: "Shipments",
                column: "PaymentId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Shipments_Status",
                table: "Shipments",
                column: "Status",
                filter: "\"Status\" NOT IN ('Delivered','Refused','Returned','Lost')");
        }
    }
}
