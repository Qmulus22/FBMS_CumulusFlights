using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FBMS_CumulusFlights.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddUserActivityLog : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "UserActivityLog",
                columns: table => new
                {
                    log_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    user_id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    login_time = table.Column<DateTime>(type: "datetime", nullable: true),
                    logout_time = table.Column<DateTime>(type: "datetime", nullable: true),
                    ip_address = table.Column<string>(type: "nvarchar(45)", maxLength: 45, nullable: true),
                    is_successful = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserActivityLog", x => x.log_id);
                    table.ForeignKey(
                        name: "FK_UserActivityLog_AspNetUsers_user_id",
                        column: x => x.user_id,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_UserActivityLog_user_id",
                table: "UserActivityLog",
                column: "user_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UserActivityLog");
        }
    }
}
