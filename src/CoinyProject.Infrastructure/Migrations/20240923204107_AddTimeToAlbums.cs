using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace CoinyProject.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddTimeToAlbums : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: new Guid("d1e010f9-c44f-4caa-b738-34d1ef85c839"));

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: new Guid("d774a36e-8f58-4536-a105-016cfa814995"));

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: new Guid("dffea982-2819-41cf-a170-e5f55894ebf7"));

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "Albums",
                type: "datetime2",
                nullable: false,
                defaultValueSql: "SYSDATETIMEOFFSET()");

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "Albums",
                type: "datetime2",
                nullable: false,
                defaultValueSql: "SYSDATETIMEOFFSET()");

            migrationBuilder.InsertData(
                table: "AspNetRoles",
                columns: new[] { "Id", "ConcurrencyStamp", "Name", "NormalizedName" },
                values: new object[,]
                {
                    { new Guid("8fb1527c-c0fb-476a-9647-4d90c132bac0"), null, "Moderator", "MODERATOR" },
                    { new Guid("9c525be7-1d6a-47fd-bf99-902616271b72"), null, "Administrator", "ADMINISTRATOR" },
                    { new Guid("d90a34a1-6f21-408c-b632-fab495cdd109"), null, "User", "USER" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: new Guid("8fb1527c-c0fb-476a-9647-4d90c132bac0"));

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: new Guid("9c525be7-1d6a-47fd-bf99-902616271b72"));

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: new Guid("d90a34a1-6f21-408c-b632-fab495cdd109"));

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "Albums");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "Albums");

            migrationBuilder.InsertData(
                table: "AspNetRoles",
                columns: new[] { "Id", "ConcurrencyStamp", "Name", "NormalizedName" },
                values: new object[,]
                {
                    { new Guid("d1e010f9-c44f-4caa-b738-34d1ef85c839"), null, "Moderator", "MODERATOR" },
                    { new Guid("d774a36e-8f58-4536-a105-016cfa814995"), null, "User", "USER" },
                    { new Guid("dffea982-2819-41cf-a170-e5f55894ebf7"), null, "Administrator", "ADMINISTRATOR" }
                });
        }
    }
}
