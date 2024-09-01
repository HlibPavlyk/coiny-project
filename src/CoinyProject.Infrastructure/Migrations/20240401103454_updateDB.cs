using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CoinyProject.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class updateDB : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Discussions_DiscussionTopic_DiscussionTopicId",
                table: "Discussions");

            migrationBuilder.DropPrimaryKey(
                name: "PK_DiscussionTopic",
                table: "DiscussionTopic");

            migrationBuilder.RenameTable(
                name: "DiscussionTopic",
                newName: "DiscussionTopics");

            migrationBuilder.AddPrimaryKey(
                name: "PK_DiscussionTopics",
                table: "DiscussionTopics",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Discussions_DiscussionTopics_DiscussionTopicId",
                table: "Discussions",
                column: "DiscussionTopicId",
                principalTable: "DiscussionTopics",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Discussions_DiscussionTopics_DiscussionTopicId",
                table: "Discussions");

            migrationBuilder.DropPrimaryKey(
                name: "PK_DiscussionTopics",
                table: "DiscussionTopics");

            migrationBuilder.RenameTable(
                name: "DiscussionTopics",
                newName: "DiscussionTopic");

            migrationBuilder.AddPrimaryKey(
                name: "PK_DiscussionTopic",
                table: "DiscussionTopic",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Discussions_DiscussionTopic_DiscussionTopicId",
                table: "Discussions",
                column: "DiscussionTopicId",
                principalTable: "DiscussionTopic",
                principalColumn: "Id");
        }
    }
}
