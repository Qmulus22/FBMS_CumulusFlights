using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FBMS_CumulusFlights.Data.Migrations
{
    /// <inheritdoc />
    public partial class UpdateFlightModelV2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ArrivalLatitude",
                table: "Flights");

            migrationBuilder.DropColumn(
                name: "ArrivalLongitude",
                table: "Flights");

            migrationBuilder.DropColumn(
                name: "DepartureLatitude",
                table: "Flights");

            migrationBuilder.DropColumn(
                name: "DepartureLongitude",
                table: "Flights");

            migrationBuilder.RenameColumn(
                name: "DepartureTime",
                table: "Flights",
                newName: "DepartureDate");

            migrationBuilder.RenameColumn(
                name: "DepartureCity",
                table: "Flights",
                newName: "OriginCountry");

            migrationBuilder.RenameColumn(
                name: "DepartureAirportName",
                table: "Flights",
                newName: "OriginCity");

            migrationBuilder.RenameColumn(
                name: "DepartureAirportCountry",
                table: "Flights",
                newName: "OriginAirportName");

            migrationBuilder.RenameColumn(
                name: "DepartureAirportCode",
                table: "Flights",
                newName: "OriginAirportCode");

            migrationBuilder.RenameColumn(
                name: "ArrivalTime",
                table: "Flights",
                newName: "ArrivalDate");

            migrationBuilder.RenameColumn(
                name: "ArrivalCity",
                table: "Flights",
                newName: "DestinationCountry");

            migrationBuilder.RenameColumn(
                name: "ArrivalAirportName",
                table: "Flights",
                newName: "DestinationCity");

            migrationBuilder.RenameColumn(
                name: "ArrivalAirportCountry",
                table: "Flights",
                newName: "DestinationAirportName");

            migrationBuilder.RenameColumn(
                name: "ArrivalAirportCode",
                table: "Flights",
                newName: "DestinationAirportCode");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "OriginCountry",
                table: "Flights",
                newName: "DepartureCity");

            migrationBuilder.RenameColumn(
                name: "OriginCity",
                table: "Flights",
                newName: "DepartureAirportName");

            migrationBuilder.RenameColumn(
                name: "OriginAirportName",
                table: "Flights",
                newName: "DepartureAirportCountry");

            migrationBuilder.RenameColumn(
                name: "OriginAirportCode",
                table: "Flights",
                newName: "DepartureAirportCode");

            migrationBuilder.RenameColumn(
                name: "DestinationCountry",
                table: "Flights",
                newName: "ArrivalCity");

            migrationBuilder.RenameColumn(
                name: "DestinationCity",
                table: "Flights",
                newName: "ArrivalAirportName");

            migrationBuilder.RenameColumn(
                name: "DestinationAirportName",
                table: "Flights",
                newName: "ArrivalAirportCountry");

            migrationBuilder.RenameColumn(
                name: "DestinationAirportCode",
                table: "Flights",
                newName: "ArrivalAirportCode");

            migrationBuilder.RenameColumn(
                name: "DepartureDate",
                table: "Flights",
                newName: "DepartureTime");

            migrationBuilder.RenameColumn(
                name: "ArrivalDate",
                table: "Flights",
                newName: "ArrivalTime");

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
        }
    }
}
