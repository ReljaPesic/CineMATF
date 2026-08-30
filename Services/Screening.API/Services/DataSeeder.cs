using Entities = Screening.API.Entities;
using Screening.API.Repositories;

namespace Screening.API.Services;

public class DataSeeder(IServiceProvider serviceProvider) : IHostedService
{
    // Ids match the hardcoded seed data in Movie.API (MovieContextSeed) and Cinema.API (DataSeeder)
    private static readonly (Guid MovieId, Guid HallId, Guid CinemaId, DateTime StartTime, Entities.ScreeningFormat Format)[] Screenings =
    [
        (Guid.Parse("ffffffff-ffff-ffff-ffff-000000000001"), Guid.Parse("bbbbbbbb-bbbb-bbbb-0001-000000000001"), Guid.Parse("cccccccc-cccc-cccc-cccc-000000000001"), new DateTime(2026, 9, 1, 18, 0, 0, DateTimeKind.Utc), Entities.ScreeningFormat.TwoD),   // Inception @ CineMax Hall 1
        (Guid.Parse("ffffffff-ffff-ffff-ffff-000000000004"), Guid.Parse("bbbbbbbb-bbbb-bbbb-0001-000000000002"), Guid.Parse("cccccccc-cccc-cccc-cccc-000000000001"), new DateTime(2026, 9, 1, 20, 30, 0, DateTimeKind.Utc), Entities.ScreeningFormat.IMAX),  // Interstellar @ CineMax Hall 2
        (Guid.Parse("ffffffff-ffff-ffff-ffff-000000000003"), Guid.Parse("bbbbbbbb-bbbb-bbbb-0003-000000000001"), Guid.Parse("cccccccc-cccc-cccc-cccc-000000000003"), new DateTime(2026, 9, 2, 19, 0, 0, DateTimeKind.Utc), Entities.ScreeningFormat.TwoD),   // The Dark Knight @ Cineplexx Hall 1
        (Guid.Parse("ffffffff-ffff-ffff-ffff-000000000002"), Guid.Parse("bbbbbbbb-bbbb-bbbb-0003-000000000002"), Guid.Parse("cccccccc-cccc-cccc-cccc-000000000003"), new DateTime(2026, 9, 2, 21, 0, 0, DateTimeKind.Utc), Entities.ScreeningFormat.ThreeD), // The Godfather @ Cineplexx Hall 2
        (Guid.Parse("ffffffff-ffff-ffff-ffff-000000000005"), Guid.Parse("bbbbbbbb-bbbb-bbbb-0001-000000000001"), Guid.Parse("cccccccc-cccc-cccc-cccc-000000000001"), new DateTime(2026, 9, 3, 22, 0, 0, DateTimeKind.Utc), Entities.ScreeningFormat.TwoD),   // The Shining @ CineMax Hall 1
    ];

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        using var scope = serviceProvider.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<IScreeningRepository>();

        var existing = await repository.GetScreeningsAsync(null, null, null);
        if (existing.Any()) return;

        foreach (var (movieId, hallId, cinemaId, startTime, format) in Screenings)
        {
            await repository.CreateScreeningAsync(new Entities.Screening
            {
                MovieId = movieId,
                HallId = hallId,
                CinemaId = cinemaId,
                StartTime = startTime,
                Format = format
            });
        }
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
