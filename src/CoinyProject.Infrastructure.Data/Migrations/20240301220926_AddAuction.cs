using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CoinyProject.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddAuction : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "AuctionBetId",
                table: "Auctions",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<float>(
                name: "BetDelta",
                table: "Auctions",
                type: "real",
                nullable: false,
                defaultValue: 0f);

            migrationBuilder.AddColumn<DateTime>(
                name: "ExpirationTime",
                table: "Auctions",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<bool>(
                name: "IsSoldEarlier",
                table: "Auctions",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<float>(
                name: "StartPrice",
                table: "Auctions",
                type: "real",
                nullable: false,
                defaultValue: 0f);

            migrationBuilder.AddColumn<DateTime>(
                name: "StartTime",
                table: "Auctions",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.CreateTable(
                name: "AuctionBet",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Price = table.Column<float>(type: "real", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AuctionBet", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AuctionBet_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Auctions_AuctionBetId",
                table: "Auctions",
                column: "AuctionBetId");

            migrationBuilder.CreateIndex(
                name: "IX_AuctionBet_UserId",
                table: "AuctionBet",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Auctions_AuctionBet_AuctionBetId",
                table: "Auctions",
                column: "AuctionBetId",
                principalTable: "AuctionBet",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Auctions_AuctionBet_AuctionBetId",
                table: "Auctions");

            migrationBuilder.DropTable(
                name: "AuctionBet");

            migrationBuilder.DropIndex(
                name: "IX_Auctions_AuctionBetId",
                table: "Auctions");

            migrationBuilder.DropColumn(
                name: "AuctionBetId",
                table: "Auctions");

            migrationBuilder.DropColumn(
                name: "BetDelta",
                table: "Auctions");

            migrationBuilder.DropColumn(
                name: "ExpirationTime",
                table: "Auctions");

            migrationBuilder.DropColumn(
                name: "IsSoldEarlier",
                table: "Auctions");

            migrationBuilder.DropColumn(
                name: "StartPrice",
                table: "Auctions");

            migrationBuilder.DropColumn(
                name: "StartTime",
                table: "Auctions");
        }
    }
}
