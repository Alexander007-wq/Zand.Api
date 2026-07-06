using System;
using Microsoft.EntityFrameworkCore.Migrations;
using HotelListing.Api.Data;
#nullable disable

namespace HotelListing.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddedBookingsTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "CreateAtUth",
                table: "ApiKeys",
                newName: "CreatedAtUth");

            migrationBuilder.AddColumn<decimal>(
                name: "PerNightRate",
                table: "Hotels",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.CreateTable(
                name: "Bookings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    HotelId = table.Column<int>(type: "int", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    CheckIn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CheckOut = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Guests = table.Column<int>(type: "int", nullable: false),
                    TotalPrice = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Bookings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Bookings_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Bookings_Hotels_HotelId",
                        column: x => x.HotelId,
                        principalTable: "Hotels",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "HotelAdmins",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    HotelId = table.Column<int>(type: "int", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HotelAdmins", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HotelAdmins_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_HotelAdmins_Hotels_HotelId",
                        column: x => x.HotelId,
                        principalTable: "Hotels",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "22a749f6-99cb-4254-91b7-b68319f3cc3f",
                column: "ConcurrencyStamp",
                value: "22a749f6-99cb-4254-91b7-b68319f3cc3f");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "8f6da150-60c9-4069-ad71-bf2815825eb7",
                column: "ConcurrencyStamp",
                value: "8f6da150-60c9-4069-ad71-bf2815825eb7");

            migrationBuilder.InsertData(
                table: "AspNetRoles",
                columns: ["Id", "ConcurrencyStamp", "Name", "NormalizedName"],
                values: ["a7715e6f-6eee-4064-99f2-b8b56975c5db", "a7715e6f-6eee-4064-99f2-b8b56975c5db", "Hotel Admin", "HOTEL ADMIN"]);

            migrationBuilder.CreateIndex(
                name: "IX_Bookings_CheckIn_CheckOut",
                table: "Bookings",
                columns: ["CheckIn", "CheckOut" ]);

            migrationBuilder.CreateIndex(
                name: "IX_Bookings_HotelId",
                table: "Bookings",
                column: "HotelId");

            migrationBuilder.CreateIndex(
                name: "IX_Bookings_UserId",
                table: "Bookings",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_HotelAdmins_HotelId",
                table: "HotelAdmins",
                column: "HotelId");

            migrationBuilder.CreateIndex(
                name: "IX_HotelAdmins_UserId",
                table: "HotelAdmins",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Bookings");

            migrationBuilder.DropTable(
                name: "HotelAdmins");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "a7715e6f-6eee-4064-99f2-b8b56975c5db");

            migrationBuilder.DropColumn(
                name: "PerNightRate",
                table: "Hotels");

            migrationBuilder.RenameColumn(
                name: "CreatedAtUth",
                table: "ApiKeys",
                newName: "CreateAtUth");

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
    }
}
