using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FBMS_CumulusFlights.Data.Migrations
{
    /// <inheritdoc />
    public partial class CreateFlightsTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Flights_Airports_ArrivalAirportId",
                table: "Flights");

            migrationBuilder.DropForeignKey(
                name: "FK_Flights_Airports_DepartureAirportId",
                table: "Flights");

            migrationBuilder.DropTable(
                name: "Airports");

            migrationBuilder.DropTable(
                name: "PendingFlights");

            migrationBuilder.DropIndex(
                name: "IX_Flights_ArrivalAirportId",
                table: "Flights");

            migrationBuilder.DropIndex(
                name: "IX_Flights_DepartureAirportId",
                table: "Flights");

            migrationBuilder.DropColumn(
                name: "ArrivalAirportId",
                table: "Flights");

            migrationBuilder.DropColumn(
                name: "DepartureAirportId",
                table: "Flights");

            migrationBuilder.DropColumn(
                name: "Source",
                table: "Flights");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "Flights");

            migrationBuilder.DropColumn(
                name: "UpdateTime",
                table: "Flights");

            migrationBuilder.AddColumn<DateTime>(
                name: "AddedAt",
                table: "Flights",
                type: "datetime2",
                nullable: false,
                defaultValueSql: "GETUTCDATE()");

            migrationBuilder.AddColumn<string>(
                name: "ApprovalStatus",
                table: "Flights",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ArrivalAirportCode",
                table: "Flights",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ArrivalAirportCountry",
                table: "Flights",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ArrivalAirportName",
                table: "Flights",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ArrivalCity",
                table: "Flights",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<decimal>(
                name: "ArrivalLatitude",
                table: "Flights",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "ArrivalLongitude",
                table: "Flights",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DepartureAirportCode",
                table: "Flights",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "DepartureAirportCountry",
                table: "Flights",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DepartureAirportName",
                table: "Flights",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "DepartureCity",
                table: "Flights",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<decimal>(
                name: "DepartureLatitude",
                table: "Flights",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "DepartureLongitude",
                table: "Flights",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "Flights",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "OperationalStatus",
                table: "Flights",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AddedAt",
                table: "Flights");

            migrationBuilder.DropColumn(
                name: "ApprovalStatus",
                table: "Flights");

            migrationBuilder.DropColumn(
                name: "ArrivalAirportCode",
                table: "Flights");

            migrationBuilder.DropColumn(
                name: "ArrivalAirportCountry",
                table: "Flights");

            migrationBuilder.DropColumn(
                name: "ArrivalAirportName",
                table: "Flights");

            migrationBuilder.DropColumn(
                name: "ArrivalCity",
                table: "Flights");

            migrationBuilder.DropColumn(
                name: "ArrivalLatitude",
                table: "Flights");

            migrationBuilder.DropColumn(
                name: "ArrivalLongitude",
                table: "Flights");

            migrationBuilder.DropColumn(
                name: "DepartureAirportCode",
                table: "Flights");

            migrationBuilder.DropColumn(
                name: "DepartureAirportCountry",
                table: "Flights");

            migrationBuilder.DropColumn(
                name: "DepartureAirportName",
                table: "Flights");

            migrationBuilder.DropColumn(
                name: "DepartureCity",
                table: "Flights");

            migrationBuilder.DropColumn(
                name: "DepartureLatitude",
                table: "Flights");

            migrationBuilder.DropColumn(
                name: "DepartureLongitude",
                table: "Flights");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "Flights");

            migrationBuilder.DropColumn(
                name: "OperationalStatus",
                table: "Flights");

            migrationBuilder.AddColumn<int>(
                name: "ArrivalAirportId",
                table: "Flights",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "DepartureAirportId",
                table: "Flights",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "Source",
                table: "Flights",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Status",
                table: "Flights",
                type: "nvarchar(15)",
                maxLength: 15,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdateTime",
                table: "Flights",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.CreateTable(
                name: "Airports",
                columns: table => new
                {
                    AirportId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AirportCode = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    AirportName = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    City = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    Country = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    Latitude = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    Longitude = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    TimeZone = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Airports", x => x.AirportId);
                });

            migrationBuilder.CreateTable(
                name: "PendingFlights",
                columns: table => new
                {
                    PendingFlightId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AddedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ArrivalAirportCode = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    ArrivalAirportName = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    ArrivalCity = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    ArrivalTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Country = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    DepartureAirportCode = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    DepartureAirportName = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    DepartureCity = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    DepartureTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DistanceKm = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    FlightNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Price = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(15)", maxLength: 15, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PendingFlights", x => x.PendingFlightId);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Flights_ArrivalAirportId",
                table: "Flights",
                column: "ArrivalAirportId");

            migrationBuilder.CreateIndex(
                name: "IX_Flights_DepartureAirportId",
                table: "Flights",
                column: "DepartureAirportId");

            migrationBuilder.AddForeignKey(
                name: "FK_Flights_Airports_ArrivalAirportId",
                table: "Flights",
                column: "ArrivalAirportId",
                principalTable: "Airports",
                principalColumn: "AirportId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Flights_Airports_DepartureAirportId",
                table: "Flights",
                column: "DepartureAirportId",
                principalTable: "Airports",
                principalColumn: "AirportId",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
