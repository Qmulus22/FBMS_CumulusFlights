using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FBMS_CumulusFlights.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddCityFieldsToPendingFlight : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ArrivalCity",
                table: "PendingFlights",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "DepartureCity",
                table: "PendingFlights",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ArrivalCity",
                table: "PendingFlights");

            migrationBuilder.DropColumn(
                name: "DepartureCity",
                table: "PendingFlights");
        }
    }
}
