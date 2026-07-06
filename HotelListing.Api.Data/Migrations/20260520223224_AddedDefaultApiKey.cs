using System;
using Microsoft.EntityFrameworkCore.Migrations;
using HotelListing.Api.Data;

#nullable disable

namespace HotelListing.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddedDefaultApiKey : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {

            migrationBuilder.InsertData(
                table: "ApiKeys",
                columns: ["Id", "AppName", "CreateAtUth", "ExpiresAtUth", "Key"],
                values: [1, "app", new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 1, 0, 0, 0)), null, "dxNlcjFAbG9jYWxob3N0LmNvbTpQQHNzd29yZDE="]);

            migrationBuilder.CreateIndex(
                name: "IX_ApiKeys_Key",
                table: "ApiKeys",
                column: "Key",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ApiKeys_Key",
                table: "ApiKeys");

            migrationBuilder.DeleteData(
                table: "ApiKeys",
                keyColumn: "Id",
                keyValue: 1);
        }
    }
}
