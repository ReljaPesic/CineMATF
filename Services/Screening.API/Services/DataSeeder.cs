using Entities = Screening.API.Entities;
using Screening.API.Repositories;

namespace Screening.API.Services;

public class DataSeeder(IServiceProvider serviceProvider) : IHostedService
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        using var scope = serviceProvider.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<IScreeningRepository>();

        var existing = await repository.GetScreeningsAsync(null, null, null);
        if (existing.Any()) return;

        // Start from tomorrow (not today) so every seeded screening is still bookable
        // regardless of what time of day the seeder happens to run.
        var today = DateTime.UtcNow.Date.AddDays(1);

        foreach (var (id, movieId, hallId, cinemaId, startTime, format) in BuildScreenings(today))
        {
            await repository.CreateScreeningAsync(new Entities.Screening
            {
                Id = id,
                MovieId = movieId,
                HallId = hallId,
                CinemaId = cinemaId,
                StartTime = startTime,
                Format = format
            });
        }
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    // Ids match the hardcoded seed data in Movie.API (MovieContextSeed) and Cinema.API (DataSeeder).
    // Dates are computed relative to seed time rather than hardcoded, so screenings never age
    // into the past and silently become unbookable.
    private static (Guid Id, Guid MovieId, Guid HallId, Guid CinemaId, DateTime StartTime, Entities.ScreeningFormat Format)[] BuildScreenings(DateTime today) =>
    [
        (Guid.Parse("77777777-7777-7777-7777-000000000001"), Guid.Parse("ffffffff-ffff-ffff-ffff-000000000001"), Guid.Parse("bbbbbbbb-bbbb-bbbb-0001-000000000001"), Guid.Parse("cccccccc-cccc-cccc-cccc-000000000001"), today.AddHours(18), Entities.ScreeningFormat.TwoD),          // Inception @ CineMax Hall 1
        (Guid.Parse("77777777-7777-7777-7777-000000000002"), Guid.Parse("ffffffff-ffff-ffff-ffff-000000000004"), Guid.Parse("bbbbbbbb-bbbb-bbbb-0001-000000000002"), Guid.Parse("cccccccc-cccc-cccc-cccc-000000000001"), today.AddHours(20.5), Entities.ScreeningFormat.IMAX),      // Interstellar @ CineMax Hall 2
        (Guid.Parse("77777777-7777-7777-7777-000000000003"), Guid.Parse("ffffffff-ffff-ffff-ffff-000000000003"), Guid.Parse("bbbbbbbb-bbbb-bbbb-0003-000000000001"), Guid.Parse("cccccccc-cccc-cccc-cccc-000000000003"), today.AddDays(1).AddHours(19), Entities.ScreeningFormat.TwoD),   // The Dark Knight @ Cineplexx Hall 1
        (Guid.Parse("77777777-7777-7777-7777-000000000004"), Guid.Parse("ffffffff-ffff-ffff-ffff-000000000002"), Guid.Parse("bbbbbbbb-bbbb-bbbb-0003-000000000002"), Guid.Parse("cccccccc-cccc-cccc-cccc-000000000003"), today.AddDays(1).AddHours(21), Entities.ScreeningFormat.ThreeD), // The Godfather @ Cineplexx Hall 2
        (Guid.Parse("77777777-7777-7777-7777-000000000005"), Guid.Parse("ffffffff-ffff-ffff-ffff-000000000005"), Guid.Parse("bbbbbbbb-bbbb-bbbb-0001-000000000001"), Guid.Parse("cccccccc-cccc-cccc-cccc-000000000001"), today.AddDays(2).AddHours(22), Entities.ScreeningFormat.TwoD),   // The Shining @ CineMax Hall 1
    ];
}
