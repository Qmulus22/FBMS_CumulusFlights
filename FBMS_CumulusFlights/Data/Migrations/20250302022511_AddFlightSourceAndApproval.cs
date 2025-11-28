using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FBMS_CumulusFlights.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddFlightSourceAndApproval : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ApprovedByUserId",
                table: "Flights",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "FlightType",
                table: "Flights",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "ReturnDate",
                table: "Flights",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "SourceType",
                table: "Flights",
                type: "int",
                nullable: false,
                defaultValue: 0);

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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
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
                name: "FlightType",
                table: "Flights");

            migrationBuilder.DropColumn(
                name: "ReturnDate",
                table: "Flights");

            migrationBuilder.DropColumn(
                name: "SourceType",
                table: "Flights");
        }
    }
}
