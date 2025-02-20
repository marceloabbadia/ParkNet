using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProjParkNet.Migrations
{
    /// <inheritdoc />
    public partial class AdjustColumnTypeVehicle : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "type_vehicle",
                table: "ParkingUsages",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "type_vehicle",
                table: "ParkingUsages");
        }
    }
}
