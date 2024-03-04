using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CoinyProject.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddFavorites : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AlbumElements_Albums_AlbumId",
                table: "AlbumElements");

            migrationBuilder.DropForeignKey(
                name: "FK_Albums_Users_UserId",
                table: "Albums");

            migrationBuilder.DropForeignKey(
                name: "FK_AuctionBet_Users_UserId",
                table: "AuctionBet");

            migrationBuilder.DropForeignKey(
                name: "FK_Auctions_AuctionBet_AuctionBetId",
                table: "Auctions");

            migrationBuilder.DropForeignKey(
                name: "FK_DiscussionMessages_Discussions_DiscussionId",
                table: "DiscussionMessages");

            migrationBuilder.DropForeignKey(
                name: "FK_DiscussionMessages_Users_UserId",
                table: "DiscussionMessages");

            migrationBuilder.DropForeignKey(
                name: "FK_Discussions_DiscussionMessages_RootQuestionId",
                table: "Discussions");

            migrationBuilder.DropIndex(
                name: "IX_Users_RoleId",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_Discussions_RootQuestionId",
                table: "Discussions");

            migrationBuilder.DropIndex(
                name: "IX_Auctions_AuctionBetId",
                table: "Auctions");

            migrationBuilder.DropIndex(
                name: "IX_Auctions_LotId",
                table: "Auctions");

            migrationBuilder.DropIndex(
                name: "IX_AlbumElements_AccessibilityId",
                table: "AlbumElements");

            migrationBuilder.DropPrimaryKey(
                name: "PK_AuctionBet",
                table: "AuctionBet");

            migrationBuilder.DropColumn(
                name: "RootQuestionId",
                table: "Discussions");

            migrationBuilder.DropColumn(
                name: "AuctionBetId",
                table: "Auctions");

            migrationBuilder.RenameTable(
                name: "AuctionBet",
                newName: "AuctionBets");

            migrationBuilder.RenameIndex(
                name: "IX_AuctionBet_UserId",
                table: "AuctionBets",
                newName: "IX_AuctionBets_UserId");

            migrationBuilder.AddColumn<string>(
                name: "RootQuestion",
                table: "Discussions",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "UserId",
                table: "Discussions",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AlterColumn<string>(
                name: "UserId",
                table: "DiscussionMessages",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "DiscussionId",
                table: "DiscussionMessages",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "UserId",
                table: "Albums",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "Albums",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<int>(
                name: "Rate",
                table: "Albums",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AlterColumn<int>(
                name: "AlbumId",
                table: "AlbumElements",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddColumn<int>(
                name: "AuctionId",
                table: "AuctionBets",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddPrimaryKey(
                name: "PK_AuctionBets",
                table: "AuctionBets",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_Users_RoleId",
                table: "Users",
                column: "RoleId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Discussions_UserId",
                table: "Discussions",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Auctions_LotId",
                table: "Auctions",
                column: "LotId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AlbumElements_AccessibilityId",
                table: "AlbumElements",
                column: "AccessibilityId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AuctionBets_AuctionId",
                table: "AuctionBets",
                column: "AuctionId");

            migrationBuilder.AddForeignKey(
                name: "FK_AlbumElements_Albums_AlbumId",
                table: "AlbumElements",
                column: "AlbumId",
                principalTable: "Albums",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Albums_Users_UserId",
                table: "Albums",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_AuctionBets_Auctions_AuctionId",
                table: "AuctionBets",
                column: "AuctionId",
                principalTable: "Auctions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_AuctionBets_Users_UserId",
                table: "AuctionBets",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_DiscussionMessages_Discussions_DiscussionId",
                table: "DiscussionMessages",
                column: "DiscussionId",
                principalTable: "Discussions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_DiscussionMessages_Users_UserId",
                table: "DiscussionMessages",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Discussions_Users_UserId",
                table: "Discussions",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AlbumElements_Albums_AlbumId",
                table: "AlbumElements");

            migrationBuilder.DropForeignKey(
                name: "FK_Albums_Users_UserId",
                table: "Albums");

            migrationBuilder.DropForeignKey(
                name: "FK_AuctionBets_Auctions_AuctionId",
                table: "AuctionBets");

            migrationBuilder.DropForeignKey(
                name: "FK_AuctionBets_Users_UserId",
                table: "AuctionBets");

            migrationBuilder.DropForeignKey(
                name: "FK_DiscussionMessages_Discussions_DiscussionId",
                table: "DiscussionMessages");

            migrationBuilder.DropForeignKey(
                name: "FK_DiscussionMessages_Users_UserId",
                table: "DiscussionMessages");

            migrationBuilder.DropForeignKey(
                name: "FK_Discussions_Users_UserId",
                table: "Discussions");

            migrationBuilder.DropIndex(
                name: "IX_Users_RoleId",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_Discussions_UserId",
                table: "Discussions");

            migrationBuilder.DropIndex(
                name: "IX_Auctions_LotId",
                table: "Auctions");

            migrationBuilder.DropIndex(
                name: "IX_AlbumElements_AccessibilityId",
                table: "AlbumElements");

            migrationBuilder.DropPrimaryKey(
                name: "PK_AuctionBets",
                table: "AuctionBets");

            migrationBuilder.DropIndex(
                name: "IX_AuctionBets_AuctionId",
                table: "AuctionBets");

            migrationBuilder.DropColumn(
                name: "RootQuestion",
                table: "Discussions");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "Discussions");

            migrationBuilder.DropColumn(
                name: "Rate",
                table: "Albums");

            migrationBuilder.DropColumn(
                name: "AuctionId",
                table: "AuctionBets");

            migrationBuilder.RenameTable(
                name: "AuctionBets",
                newName: "AuctionBet");

            migrationBuilder.RenameIndex(
                name: "IX_AuctionBets_UserId",
                table: "AuctionBet",
                newName: "IX_AuctionBet_UserId");

            migrationBuilder.AddColumn<int>(
                name: "RootQuestionId",
                table: "Discussions",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AlterColumn<string>(
                name: "UserId",
                table: "DiscussionMessages",
                type: "nvarchar(450)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AlterColumn<int>(
                name: "DiscussionId",
                table: "DiscussionMessages",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<int>(
                name: "AuctionBetId",
                table: "Auctions",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AlterColumn<string>(
                name: "UserId",
                table: "Albums",
                type: "nvarchar(450)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "Albums",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "AlbumId",
                table: "AlbumElements",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddPrimaryKey(
                name: "PK_AuctionBet",
                table: "AuctionBet",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_Users_RoleId",
                table: "Users",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "IX_Discussions_RootQuestionId",
                table: "Discussions",
                column: "RootQuestionId");

            migrationBuilder.CreateIndex(
                name: "IX_Auctions_AuctionBetId",
                table: "Auctions",
                column: "AuctionBetId");

            migrationBuilder.CreateIndex(
                name: "IX_Auctions_LotId",
                table: "Auctions",
                column: "LotId");

            migrationBuilder.CreateIndex(
                name: "IX_AlbumElements_AccessibilityId",
                table: "AlbumElements",
                column: "AccessibilityId");

            migrationBuilder.AddForeignKey(
                name: "FK_AlbumElements_Albums_AlbumId",
                table: "AlbumElements",
                column: "AlbumId",
                principalTable: "Albums",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Albums_Users_UserId",
                table: "Albums",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_AuctionBet_Users_UserId",
                table: "AuctionBet",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Auctions_AuctionBet_AuctionBetId",
                table: "Auctions",
                column: "AuctionBetId",
                principalTable: "AuctionBet",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_DiscussionMessages_Discussions_DiscussionId",
                table: "DiscussionMessages",
                column: "DiscussionId",
                principalTable: "Discussions",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_DiscussionMessages_Users_UserId",
                table: "DiscussionMessages",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Discussions_DiscussionMessages_RootQuestionId",
                table: "Discussions",
                column: "RootQuestionId",
                principalTable: "DiscussionMessages",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
