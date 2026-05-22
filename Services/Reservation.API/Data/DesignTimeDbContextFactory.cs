using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Reservation.API.Data;

public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<ReservationDbContext>
{
    public ReservationDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<ReservationDbContext>();
        optionsBuilder.UseNpgsql("Host=localhost;Database=ReservationServiceDb;Username=postgres;Password=postgres");

        return new ReservationDbContext(optionsBuilder.Options);
    }
}
