using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Reservation.API.Data;

public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<ReservationDbContext>
{
    public ReservationDbContext CreateDbContext(string[] args)
    {
        DotNetEnv.Env.Load();

        var connectionString = Environment.GetEnvironmentVariable("CONNECTION_STRING")
            ?? "Host=localhost;Database=ReservationServiceDb;Username=postgres;Password=postgres";

        var optionsBuilder = new DbContextOptionsBuilder<ReservationDbContext>();
        optionsBuilder.UseNpgsql(connectionString);

        return new ReservationDbContext(optionsBuilder.Options);
    }
}
