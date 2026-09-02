using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Reservation.API.Data;
using Reservation.API.ExternalServices;
using Reservation.API.Repositories;
using Reservation.API.Services;

namespace Reservation.API.Tests.Integration;

// Swaps the Postgres-backed DbContext for an in-memory one, and the outbound
// Cinema/Movie/Screening clients for mocks, so ownership checks can be exercised
// end-to-end through the real controllers/middleware without any other service running.
public class ReservationApiFactory : WebApplicationFactory<Program>
{
    private readonly string _dbName = $"TestDb_{Guid.NewGuid():N}";

    public Mock<ICinemaApiClient> CinemaApiClientMock { get; } = new();
    public Mock<IMovieApiClient> MovieApiClientMock { get; } = new();
    public Mock<IScreeningApiClient> ScreeningApiClientMock { get; } = new();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureServices(services =>
        {
            var allDescriptors = services.ToList();
            foreach (var descriptor in allDescriptors)
            {
                var serviceType = descriptor.ServiceType;
                var implType = descriptor.ImplementationType;

                if (implType == typeof(ReservationDbContext) ||
                    serviceType == typeof(DbContextOptions<ReservationDbContext>) ||
                    serviceType == typeof(IReservationRepository) ||
                    serviceType == typeof(IReservationService) ||
                    serviceType == typeof(ICinemaApiClient) ||
                    serviceType == typeof(IMovieApiClient) ||
                    serviceType == typeof(IScreeningApiClient))
                {
                    services.Remove(descriptor);
                    continue;
                }

                if (implType?.Assembly.FullName?.Contains("Npgsql") == true ||
                    implType?.Assembly.FullName?.Contains("Microsoft.EntityFrameworkCore") == true)
                {
                    if (serviceType.FullName?.Contains("Npgsql") == true ||
                        serviceType.FullName?.Contains("Relational") == true)
                    {
                        services.Remove(descriptor);
                    }
                }
            }

            // ReservationRepository opens explicit transactions (unsupported by the
            // in-memory provider); they're a no-op here, so just silence the warning.
            var newOptions = new DbContextOptionsBuilder<ReservationDbContext>()
                .UseInMemoryDatabase(_dbName)
                .ConfigureWarnings(w => w.Ignore(InMemoryEventId.TransactionIgnoredWarning))
                .Options;

            services.AddSingleton(newOptions);
            services.AddDbContext<ReservationDbContext>();

            services.AddScoped<IReservationRepository, ReservationRepository>();
            services.AddScoped<IReservationService, ReservationService>();

            services.AddSingleton(CinemaApiClientMock.Object);
            services.AddSingleton(MovieApiClientMock.Object);
            services.AddSingleton(ScreeningApiClientMock.Object);
        });
    }

    public async Task SeedAsync(Action<ReservationDbContext> seed)
    {
        using var scope = Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ReservationDbContext>();
        seed(db);
        await db.SaveChangesAsync();
    }
}
