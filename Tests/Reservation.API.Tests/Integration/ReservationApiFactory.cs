using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Reservation.API.Data;
using Reservation.API.ExternalServices;

namespace Reservation.API.Tests.Integration;

public class ReservationApiFactory : WebApplicationFactory<Program>
{
    private readonly string _dbName = $"TestDb_{Guid.NewGuid():N}";

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureServices(services =>
        {
            foreach (var descriptor in services.Where(d =>
                d.ServiceType == typeof(ICinemaApiClient) ||
                d.ServiceType == typeof(IMovieApiClient) ||
                d.ServiceType == typeof(IScreeningApiClient) ||
                d.ServiceType == typeof(IHostedService)).ToList())
            {
                services.Remove(descriptor);
            }

            // Swap the real Postgres provider for InMemory. AddDbContext registers the
            // "UseNpgsql" configure action as an IDbContextOptionsConfiguration<T> entry
            // rather than a type we can match by name, and EF applies every registered
            // configuration - Npgsql's and ours - to the same DbContextOptions unless the
            // old one is removed first, which is what trips "two providers registered".
            foreach (var descriptor in services.Where(d =>
                d.ImplementationType == typeof(ReservationDbContext) ||
                d.ServiceType == typeof(DbContextOptions<ReservationDbContext>) ||
                d.ServiceType == typeof(IDbContextOptionsConfiguration<ReservationDbContext>)).ToList())
            {
                services.Remove(descriptor);
            }

            // The repository wraps reservation creation/cancellation in a transaction;
            // the in-memory provider doesn't support real transactions and by default
            // throws on that instead of just no-op'ing, so silence the warning.
            services.AddDbContext<ReservationDbContext>(options => options
                .UseInMemoryDatabase(_dbName)
                .ConfigureWarnings(w => w.Ignore(InMemoryEventId.TransactionIgnoredWarning)));

            // Stand in for Cinema.API/Movie.API/Screening.API so reservation creation
            // doesn't need those services running.
            services.AddSingleton<ICinemaApiClient, FakeCinemaApiClient>();
            services.AddSingleton<IMovieApiClient, FakeMovieApiClient>();
            services.AddSingleton<IScreeningApiClient, FakeScreeningApiClient>();
        });
    }
}
