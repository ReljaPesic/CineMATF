using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Cinema.API.Migrations
{
    /// <inheritdoc />
    public partial class AddSeatTypeToSeat : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Seats_HallId",
                table: "Seats");

            migrationBuilder.DropIndex(
                name: "IX_Halls_CinemaId",
                table: "Halls");

            migrationBuilder.AddColumn<int>(
                name: "SeatType",
                table: "Seats",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Seats_HallId_Row_Number",
                table: "Seats",
                columns: new[] { "HallId", "Row", "Number" });

            migrationBuilder.CreateIndex(
                name: "IX_MovieTheatres_City",
                table: "MovieTheatres",
                column: "City");

            migrationBuilder.CreateIndex(
                name: "IX_Halls_CinemaId_Name",
                table: "Halls",
                columns: new[] { "CinemaId", "Name" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Seats_HallId_Row_Number",
                table: "Seats");

            migrationBuilder.DropIndex(
                name: "IX_MovieTheatres_City",
                table: "MovieTheatres");

            migrationBuilder.DropIndex(
                name: "IX_Halls_CinemaId_Name",
                table: "Halls");

            migrationBuilder.DropColumn(
                name: "SeatType",
                table: "Seats");

            migrationBuilder.CreateIndex(
                name: "IX_Seats_HallId",
                table: "Seats",
                column: "HallId");

            migrationBuilder.CreateIndex(
                name: "IX_Halls_CinemaId",
                table: "Halls",
                column: "CinemaId");
        }
    }
}
