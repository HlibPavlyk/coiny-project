using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Coiny.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddPaymentsShipmentsAndStripeWebhooks : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Payments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    LotId = table.Column<Guid>(type: "uuid", nullable: false),
                    BuyerId = table.Column<Guid>(type: "uuid", nullable: false),
                    SellerId = table.Column<Guid>(type: "uuid", nullable: false),
                    AmountUahKopiykas = table.Column<long>(type: "bigint", nullable: false),
                    AmountUsdCents = table.Column<long>(type: "bigint", nullable: false),
                    RateUsedUahPerUsd = table.Column<decimal>(type: "numeric(10,4)", nullable: false),
                    StripePaymentIntentId = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Status = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    DueAt = table.Column<DateTime>(type: "timestamptz", nullable: false),
                    AuthorizedAt = table.Column<DateTime>(type: "timestamptz", nullable: true),
                    CapturedAt = table.Column<DateTime>(type: "timestamptz", nullable: true),
                    CancelledAt = table.Column<DateTime>(type: "timestamptz", nullable: true),
                    LastWebhookEventId = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamptz", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamptz", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Payments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Payments_AspNetUsers_BuyerId",
                        column: x => x.BuyerId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Payments_AspNetUsers_SellerId",
                        column: x => x.SellerId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Payments_Lots_LotId",
                        column: x => x.LotId,
                        principalTable: "Lots",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "StripeWebhookEvents",
                columns: table => new
                {
                    Id = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    EventType = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    ReceivedAt = table.Column<DateTime>(type: "timestamptz", nullable: false),
                    ProcessedAt = table.Column<DateTime>(type: "timestamptz", nullable: true),
                    PayloadJson = table.Column<string>(type: "jsonb", nullable: false, defaultValue: "{}"),
                    ProcessingError = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    AttemptCount = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StripeWebhookEvents", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Shipments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PaymentId = table.Column<Guid>(type: "uuid", nullable: false),
                    LotId = table.Column<Guid>(type: "uuid", nullable: false),
                    BuyerId = table.Column<Guid>(type: "uuid", nullable: false),
                    SellerId = table.Column<Guid>(type: "uuid", nullable: false),
                    NovaPoshtaTtn = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    IntDocNumber = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    SenderCityRef = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    SenderWarehouseRef = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    RecipientCityRef = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    RecipientWarehouseRef = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    RecipientName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    RecipientPhone = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    DeclaredValueUahKopiykas = table.Column<long>(type: "bigint", nullable: false),
                    Status = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    LastNpStatusCode = table.Column<int>(type: "integer", nullable: false),
                    DeliveredAt = table.Column<DateTime>(type: "timestamptz", nullable: true),
                    LastPolledAt = table.Column<DateTime>(type: "timestamptz", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamptz", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamptz", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Shipments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Shipments_AspNetUsers_BuyerId",
                        column: x => x.BuyerId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Shipments_AspNetUsers_SellerId",
                        column: x => x.SellerId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Shipments_Lots_LotId",
                        column: x => x.LotId,
                        principalTable: "Lots",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Shipments_Payments_PaymentId",
                        column: x => x.PaymentId,
                        principalTable: "Payments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ShipmentEvents",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ShipmentId = table.Column<Guid>(type: "uuid", nullable: false),
                    Status = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    NpStatusCode = table.Column<int>(type: "integer", nullable: false),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    ObservedAt = table.Column<DateTime>(type: "timestamptz", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ShipmentEvents", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ShipmentEvents_Shipments_ShipmentId",
                        column: x => x.ShipmentId,
                        principalTable: "Shipments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Payments_BuyerId",
                table: "Payments",
                column: "BuyerId");

            migrationBuilder.CreateIndex(
                name: "IX_Payments_LotId",
                table: "Payments",
                column: "LotId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Payments_SellerId",
                table: "Payments",
                column: "SellerId");

            migrationBuilder.CreateIndex(
                name: "IX_Payments_Status_DueAt",
                table: "Payments",
                columns: new[] { "Status", "DueAt" },
                filter: "\"Status\" = 'PendingAuthorization'");

            migrationBuilder.CreateIndex(
                name: "IX_Payments_StripePaymentIntentId",
                table: "Payments",
                column: "StripePaymentIntentId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ShipmentEvents_ShipmentId_NpStatusCode_ObservedAt",
                table: "ShipmentEvents",
                columns: new[] { "ShipmentId", "NpStatusCode", "ObservedAt" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ShipmentEvents_ShipmentId_ObservedAt",
                table: "ShipmentEvents",
                columns: new[] { "ShipmentId", "ObservedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_Shipments_BuyerId",
                table: "Shipments",
                column: "BuyerId");

            migrationBuilder.CreateIndex(
                name: "IX_Shipments_LotId",
                table: "Shipments",
                column: "LotId");

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
                name: "IX_Shipments_SellerId",
                table: "Shipments",
                column: "SellerId");

            migrationBuilder.CreateIndex(
                name: "IX_Shipments_Status",
                table: "Shipments",
                column: "Status",
                filter: "\"Status\" NOT IN ('Delivered','Refused','Returned','Lost')");

            migrationBuilder.CreateIndex(
                name: "IX_StripeWebhookEvent_Pending",
                table: "StripeWebhookEvents",
                column: "Id",
                filter: "\"ProcessedAt\" IS NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ShipmentEvents");

            migrationBuilder.DropTable(
                name: "StripeWebhookEvents");

            migrationBuilder.DropTable(
                name: "Shipments");

            migrationBuilder.DropTable(
                name: "Payments");
        }
    }
}
