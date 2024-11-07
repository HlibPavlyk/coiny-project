using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace CoinyProject.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateImage : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: new Guid("58260ef7-6411-4b12-af43-1c8c301537f5"));

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: new Guid("e65c3a6b-ccf1-4d58-82c6-bd9ca5ed130e"));

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: new Guid("fbf775ff-c6bb-49e4-ab74-81fa8d8c013b"));

            migrationBuilder.DropColumn(
                name: "Image",
                table: "AlbumElements");

            migrationBuilder.AddColumn<string>(
                name: "ImageUrl",
                table: "AlbumElements",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.InsertData(
                table: "AspNetRoles",
                columns: new[] { "Id", "ConcurrencyStamp", "Name", "NormalizedName" },
                values: new object[,]
                {
                    { new Guid("144141ed-ae57-41cf-929a-a4b392681344"), null, "User", "USER" },
                    { new Guid("60d47e83-2ec4-468e-9d41-b2ebe826e016"), null, "Moderator", "MODERATOR" },
                    { new Guid("ec60c9dd-58d0-4f32-a796-71e47955e387"), null, "Administrator", "ADMINISTRATOR" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: new Guid("144141ed-ae57-41cf-929a-a4b392681344"));

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: new Guid("60d47e83-2ec4-468e-9d41-b2ebe826e016"));

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: new Guid("ec60c9dd-58d0-4f32-a796-71e47955e387"));

            migrationBuilder.DropColumn(
                name: "ImageUrl",
                table: "AlbumElements");

            migrationBuilder.AddColumn<byte[]>(
                name: "Image",
                table: "AlbumElements",
                type: "varbinary(max)",
                nullable: false,
                defaultValue: new byte[0]);

            migrationBuilder.InsertData(
                table: "AspNetRoles",
                columns: new[] { "Id", "ConcurrencyStamp", "Name", "NormalizedName" },
                values: new object[,]
                {
                    { new Guid("58260ef7-6411-4b12-af43-1c8c301537f5"), null, "User", "USER" },
                    { new Guid("e65c3a6b-ccf1-4d58-82c6-bd9ca5ed130e"), null, "Moderator", "MODERATOR" },
                    { new Guid("fbf775ff-c6bb-49e4-ab74-81fa8d8c013b"), null, "Administrator", "ADMINISTRATOR" }
                });
        }
    }
}
