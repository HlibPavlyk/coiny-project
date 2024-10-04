using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace CoinyProject.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddRoles : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
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
        }
    }
}
