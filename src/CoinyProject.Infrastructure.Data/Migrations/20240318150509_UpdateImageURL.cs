using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CoinyProject.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class UpdateImageURL : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Image",
                table: "AlbumElements");

            migrationBuilder.AddColumn<string>(
                name: "ImageURL",
                table: "AlbumElements",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ImageURL",
                table: "AlbumElements");

            migrationBuilder.AddColumn<byte[]>(
                name: "Image",
                table: "AlbumElements",
                type: "varbinary(max)",
                nullable: true);
        }
    }
}
