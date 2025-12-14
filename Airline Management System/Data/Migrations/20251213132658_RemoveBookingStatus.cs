using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Airline_Management_System.Data.Migrations
{
    /// <inheritdoc />
    public partial class RemoveBookingStatus : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Status2",
                table: "Bookings");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Status2",
                table: "Bookings",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }
    }
}
