using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace CoinyProject.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateConfigs : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
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

            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "Discussions",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "Active",
                oldClrType: typeof(int),
                oldType: "int",
                oldDefaultValue: 1);

            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "Auctions",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "Active",
                oldClrType: typeof(int),
                oldType: "int",
                oldDefaultValue: 0);

            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "AuctionBets",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "Winning",
                oldClrType: typeof(int),
                oldType: "int",
                oldDefaultValue: 1);

            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "Albums",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "NotApproved",
                oldClrType: typeof(int),
                oldType: "int",
                oldDefaultValue: 0);

            migrationBuilder.InsertData(
                table: "AspNetRoles",
                columns: new[] { "Id", "ConcurrencyStamp", "Name", "NormalizedName" },
                values: new object[,]
                {
                    { new Guid("d1e010f9-c44f-4caa-b738-34d1ef85c839"), null, "Moderator", "MODERATOR" },
                    { new Guid("d774a36e-8f58-4536-a105-016cfa814995"), null, "User", "USER" },
                    { new Guid("dffea982-2819-41cf-a170-e5f55894ebf7"), null, "Administrator", "ADMINISTRATOR" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUsers_Email",
                table: "AspNetUsers",
                column: "Email",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_AspNetUsers_Email",
                table: "AspNetUsers");

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

            migrationBuilder.AlterColumn<int>(
                name: "Status",
                table: "Discussions",
                type: "int",
                nullable: false,
                defaultValue: 1,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldDefaultValue: "Active");

            migrationBuilder.AlterColumn<int>(
                name: "Status",
                table: "Auctions",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldDefaultValue: "Active");

            migrationBuilder.AlterColumn<int>(
                name: "Status",
                table: "AuctionBets",
                type: "int",
                nullable: false,
                defaultValue: 1,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldDefaultValue: "Winning");

            migrationBuilder.AlterColumn<int>(
                name: "Status",
                table: "Albums",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldDefaultValue: "NotApproved");

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
    }
}
