using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FBMS_CumulusFlights.Data.Migrations
{
    /// <inheritdoc />
    public partial class MoveReservationColoumnToTicket : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BaggageType",
                table: "Reservations");

            migrationBuilder.DropColumn(
                name: "SeatClass",
                table: "Reservations");

            migrationBuilder.DropColumn(
                name: "SeatType",
                table: "Reservations");

            migrationBuilder.AddColumn<string>(
                name: "BaggageType",
                table: "Tickets",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "SeatClass",
                table: "Tickets",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "SeatType",
                table: "Tickets",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BaggageType",
                table: "Tickets");

            migrationBuilder.DropColumn(
                name: "SeatClass",
                table: "Tickets");

            migrationBuilder.DropColumn(
                name: "SeatType",
                table: "Tickets");

            migrationBuilder.AddColumn<string>(
                name: "BaggageType",
                table: "Reservations",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "SeatClass",
                table: "Reservations",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "SeatType",
                table: "Reservations",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }
    }
}
