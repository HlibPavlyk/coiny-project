using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace CoinyProject.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateStatusEntities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
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

            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "Albums",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldDefaultValue: "NotApproved");

            migrationBuilder.AlterColumn<string>(
                name: "ImageUrl",
                table: "AlbumElements",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<string>(
                name: "Status",
                table: "AlbumElements",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "NotApproved");

            migrationBuilder.InsertData(
                table: "AspNetRoles",
                columns: new[] { "Id", "ConcurrencyStamp", "Name", "NormalizedName" },
                values: new object[,]
                {
                    { new Guid("2aa6dec2-72f2-442c-8cef-e1f1c267f99f"), null, "Moderator", "MODERATOR" },
                    { new Guid("6a3e439b-ebe0-4fc3-ae1f-bfbf407ce4a4"), null, "User", "USER" },
                    { new Guid("6fc06e14-0e1d-436f-ba30-ae66c96aa9aa"), null, "Administrator", "ADMINISTRATOR" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: new Guid("2aa6dec2-72f2-442c-8cef-e1f1c267f99f"));

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: new Guid("6a3e439b-ebe0-4fc3-ae1f-bfbf407ce4a4"));

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: new Guid("6fc06e14-0e1d-436f-ba30-ae66c96aa9aa"));

            migrationBuilder.DropColumn(
                name: "Status",
                table: "AlbumElements");

            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "Albums",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "NotApproved",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "ImageUrl",
                table: "AlbumElements",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

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
    }
}
