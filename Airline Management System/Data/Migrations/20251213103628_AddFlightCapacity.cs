using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Airline_Management_System.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddFlightCapacity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Capacity",
                table: "Flights",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Capacity",
                table: "Flights");
        }
    }
}
