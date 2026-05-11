using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Coiny.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddBids : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Bids",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    LotId = table.Column<Guid>(type: "uuid", nullable: false),
                    BidderId = table.Column<Guid>(type: "uuid", nullable: false),
                    AmountUahKopiykas = table.Column<long>(type: "bigint", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamptz", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Bids", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Bids_AspNetUsers_BidderId",
                        column: x => x.BidderId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Bids_Lots_LotId",
                        column: x => x.LotId,
                        principalTable: "Lots",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Lots_WinningBidId",
                table: "Lots",
                column: "WinningBidId");

            migrationBuilder.CreateIndex(
                name: "IX_Bids_BidderId_CreatedAt",
                table: "Bids",
                columns: new[] { "BidderId", "CreatedAt" },
                descending: new[] { false, true });

            migrationBuilder.CreateIndex(
                name: "IX_Bids_LotId_AmountUahKopiykas",
                table: "Bids",
                columns: new[] { "LotId", "AmountUahKopiykas" },
                descending: new[] { false, true });

            migrationBuilder.AddForeignKey(
                name: "FK_Lots_Bids_WinningBidId",
                table: "Lots",
                column: "WinningBidId",
                principalTable: "Bids",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Lots_Bids_WinningBidId",
                table: "Lots");

            migrationBuilder.DropTable(
                name: "Bids");

            migrationBuilder.DropIndex(
                name: "IX_Lots_WinningBidId",
                table: "Lots");
        }
    }
}
