using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Reservation.API.Migrations
{
    /// <inheritdoc />
    public partial class FixSeatRowToInt : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("ALTER TABLE \"Tickets\" ALTER COLUMN \"SeatRow\" TYPE integer USING \"SeatRow\"::integer;");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("ALTER TABLE \"Tickets\" ALTER COLUMN \"SeatRow\" TYPE text USING \"SeatRow\"::text;");
        }
    }
}
