using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FBMS_CumulusFlights.Data.Migrations
{
    /// <inheritdoc />
    public partial class UpdateFlightModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Flights_AspNetUsers_ApprovedByUserId",
                table: "Flights");

            migrationBuilder.DropIndex(
                name: "IX_Flights_ApprovedByUserId",
                table: "Flights");

            migrationBuilder.DropColumn(
                name: "ApprovedByUserId",
                table: "Flights");

            migrationBuilder.DropColumn(
                name: "ArrivalDate",
                table: "Flights");

            migrationBuilder.DropColumn(
                name: "DestinationCountry",
                table: "Flights");

            migrationBuilder.DropColumn(
                name: "OriginCountry",
                table: "Flights");

            migrationBuilder.DropColumn(
                name: "ReturnDate",
                table: "Flights");

            migrationBuilder.AddColumn<double>(
                name: "FlightDuration",
                table: "Flights",
                type: "float",
                nullable: false,
                defaultValue: 0.0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FlightDuration",
                table: "Flights");

            migrationBuilder.AddColumn<string>(
                name: "ApprovedByUserId",
                table: "Flights",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ArrivalDate",
                table: "Flights",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "DestinationCountry",
                table: "Flights",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "OriginCountry",
                table: "Flights",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ReturnDate",
                table: "Flights",
                type: "datetime2",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Flights_ApprovedByUserId",
                table: "Flights",
                column: "ApprovedByUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Flights_AspNetUsers_ApprovedByUserId",
                table: "Flights",
                column: "ApprovedByUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }
    }
}
