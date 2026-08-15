using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Reservation.API.Migrations
{
    /// <inheritdoc />
    public partial class AddUniqueSeatLockIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_SeatLocks_ScreeningId_SeatId",
                table: "SeatLocks");

            migrationBuilder.CreateIndex(
                name: "IX_SeatLocks_ScreeningId_SeatId",
                table: "SeatLocks",
                columns: new[] { "ScreeningId", "SeatId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_SeatLocks_ScreeningId_SeatId",
                table: "SeatLocks");

            migrationBuilder.CreateIndex(
                name: "IX_SeatLocks_ScreeningId_SeatId",
                table: "SeatLocks",
                columns: new[] { "ScreeningId", "SeatId" });
        }
    }
}
