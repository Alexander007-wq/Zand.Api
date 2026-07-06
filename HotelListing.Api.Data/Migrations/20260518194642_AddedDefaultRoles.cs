using Microsoft.EntityFrameworkCore.Migrations;
using HotelListing.Api.Data;

#nullable disable


namespace HotelListing.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddedDefaultRoles : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "AspNetRoles",
                columns: ["Id", "ConcurrencyStamp", "Name", "NormalizedName" ],
                values: new object[,]
                {
                    { "36aac992-72ff-4527-9008-52e7c145ca39", null, "User", "USER" },
                    { "c78e8f15-6a6c-4c8a-b5d1-98394b071953", null, "Administrator", "ADMINISTRATOR" }
                });
       
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "22a749f6-99cb-4254-91b7-b68319f3cc3f");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "8f6da150-60c9-4069-ad71-bf2815825eb7");
        }
    }
}
