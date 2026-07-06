using Microsoft.EntityFrameworkCore.Migrations;
using HotelListing.Api.Data;

#nullable disable

namespace HotelListing.Api.Migrations
{
    /// <inheritdoc />
    public partial class FixStaticRoleSeedData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ApiKeys_Key",
                table: "ApiKeys");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "22a749f6-99cb-4254-91b7-b68319f3cc3f",
                column: "ConcurrencyStamp",
                value: "");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "8f6da150-60c9-4069-ad71-bf2815825eb7",
                column: "ConcurrencyStamp",
                value: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "22a749f6-99cb-4254-91b7-b68319f3cc3f",
                column: "ConcurrencyStamp",
                value: "5f90821b-68e0-4b10-97f7-81ea329476d9");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "8f6da150-60c9-4069-ad71-bf2815825eb7",
                column: "ConcurrencyStamp",
                value: "555982fc-0157-47a6-9601-bdbeecdbef62");

            migrationBuilder.CreateIndex(
                name: "IX_ApiKeys_Key",
                table: "ApiKeys",
                column: "Key",
                unique: true);
        }
    }
}
