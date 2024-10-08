using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace CoinyProject.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddElementTime : Migration
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

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "AlbumElements",
                type: "datetime2",
                nullable: false,
                defaultValueSql: "SYSDATETIMEOFFSET()");

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "AlbumElements",
                type: "datetime2",
                nullable: false,
                defaultValueSql: "SYSDATETIMEOFFSET()");

            migrationBuilder.InsertData(
                table: "AspNetRoles",
                columns: new[] { "Id", "ConcurrencyStamp", "Name", "NormalizedName" },
                values: new object[,]
                {
                    { new Guid("15e32c1a-3349-4132-830f-806b63afbefc"), null, "User", "USER" },
                    { new Guid("57445230-6cf6-4024-9118-0ac78afaaabe"), null, "Administrator", "ADMINISTRATOR" },
                    { new Guid("da04faea-c270-4e00-ab8f-fe150c26822d"), null, "Moderator", "MODERATOR" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: new Guid("15e32c1a-3349-4132-830f-806b63afbefc"));

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: new Guid("57445230-6cf6-4024-9118-0ac78afaaabe"));

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: new Guid("da04faea-c270-4e00-ab8f-fe150c26822d"));

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "Albums");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "Albums");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "AlbumElements");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "AlbumElements");

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
