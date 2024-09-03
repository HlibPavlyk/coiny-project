using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CoinyProject.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class update : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AlbumElements_Albums_AlbumId",
                table: "AlbumElements");

            migrationBuilder.DropForeignKey(
                name: "FK_AuctionBet_Users_UserId",
                table: "AuctionBet");

            migrationBuilder.DropForeignKey(
                name: "FK_Auctions_AlbumElements_LotId",
                table: "Auctions");

            migrationBuilder.DropForeignKey(
                name: "FK_Auctions_AuctionBet_AuctionBetId",
                table: "Auctions");

            migrationBuilder.DropForeignKey(
                name: "FK_DiscussionMessages_Discussions_DiscussionId",
                table: "DiscussionMessages");

            migrationBuilder.DropForeignKey(
                name: "FK_Discussions_DiscussionMessages_RootQuestionId",
                table: "Discussions");

            migrationBuilder.DropForeignKey(
                name: "FK_Users_UserRoles_RoleId",
                table: "Users");

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

            migrationBuilder.RenameColumn(
                name: "RoleId",
                table: "Users",
                newName: "UserRoleId");

            migrationBuilder.RenameColumn(
                name: "LotId",
                table: "Auctions",
                newName: "AlbumElementId");

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
                nullable: true);

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

            migrationBuilder.AlterColumn<string>(
                name: "UserId",
                table: "AuctionBets",
                type: "nvarchar(450)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

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

            migrationBuilder.CreateTable(
                name: "FavoriteAlbumsElement",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    AlbumId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FavoriteAlbums", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FavoriteAlbums_Albums_AlbumId",
                        column: x => x.AlbumId,
                        principalTable: "Albums",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_FavoriteAlbums_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_Users_UserRoleId",
                table: "Users",
                column: "UserRoleId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Discussions_UserId",
                table: "Discussions",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Auctions_AlbumElementId",
                table: "Auctions",
                column: "AlbumElementId",
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

            migrationBuilder.CreateIndex(
                name: "IX_FavoriteAlbums_AlbumId",
                table: "FavoriteAlbumsElement",
                column: "AlbumId");

            migrationBuilder.CreateIndex(
                name: "IX_FavoriteAlbums_UserId",
                table: "FavoriteAlbumsElement",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_AlbumElements_Albums_AlbumId",
                table: "AlbumElements",
                column: "AlbumId",
                principalTable: "Albums",
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
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Auctions_AlbumElements_AlbumElementId",
                table: "Auctions",
                column: "AlbumElementId",
                principalTable: "AlbumElements",
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
                name: "FK_Discussions_Users_UserId",
                table: "Discussions",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Users_UserRoles_UserRoleId",
                table: "Users",
                column: "UserRoleId",
                principalTable: "UserRoles",
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
                name: "FK_AuctionBets_Auctions_AuctionId",
                table: "AuctionBets");

            migrationBuilder.DropForeignKey(
                name: "FK_AuctionBets_Users_UserId",
                table: "AuctionBets");

            migrationBuilder.DropForeignKey(
                name: "FK_Auctions_AlbumElements_AlbumElementId",
                table: "Auctions");

            migrationBuilder.DropForeignKey(
                name: "FK_DiscussionMessages_Discussions_DiscussionId",
                table: "DiscussionMessages");

            migrationBuilder.DropForeignKey(
                name: "FK_Discussions_Users_UserId",
                table: "Discussions");

            migrationBuilder.DropForeignKey(
                name: "FK_Users_UserRoles_UserRoleId",
                table: "Users");

            migrationBuilder.DropTable(
                name: "FavoriteAlbumsElement");

            migrationBuilder.DropIndex(
                name: "IX_Users_UserRoleId",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_Discussions_UserId",
                table: "Discussions");

            migrationBuilder.DropIndex(
                name: "IX_Auctions_AlbumElementId",
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

            migrationBuilder.RenameColumn(
                name: "UserRoleId",
                table: "Users",
                newName: "RoleId");

            migrationBuilder.RenameColumn(
                name: "AlbumElementId",
                table: "Auctions",
                newName: "LotId");

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

            migrationBuilder.AlterColumn<string>(
                name: "UserId",
                table: "AuctionBet",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldNullable: true);

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
                name: "FK_AuctionBet_Users_UserId",
                table: "AuctionBet",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Auctions_AlbumElements_LotId",
                table: "Auctions",
                column: "LotId",
                principalTable: "AlbumElements",
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
                name: "FK_Discussions_DiscussionMessages_RootQuestionId",
                table: "Discussions",
                column: "RootQuestionId",
                principalTable: "DiscussionMessages",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Users_UserRoles_RoleId",
                table: "Users",
                column: "RoleId",
                principalTable: "UserRoles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
