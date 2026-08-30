using Entities = Screening.API.Entities;
using Screening.API.Repositories;

namespace Screening.API.Services;

public class DataSeeder(IServiceProvider serviceProvider) : IHostedService
{
    // Real ids pulled from Movie.API and Cinema.API's own seed data (docker compose up moviedb movieapi cinemadb cinemaapi)
    private static readonly (Guid MovieId, Guid HallId, Guid CinemaId, DateTime StartTime, Entities.ScreeningFormat Format)[] Screenings =
    [
        (Guid.Parse("9f22014b-339f-4ba5-9100-c815eb5ad4ed"), Guid.Parse("c80cc2fe-a9d2-41fc-9fe0-85eacb9911aa"), Guid.Parse("aea9bcc4-6476-44ff-9778-f4bb207abbe3"), new DateTime(2026, 9, 1, 18, 0, 0, DateTimeKind.Utc), Entities.ScreeningFormat.TwoD),   // Inception @ CineMax Hall 1
        (Guid.Parse("ac6f94d5-16b7-4104-a81c-ce7c3209664a"), Guid.Parse("4399dd07-0b94-4166-acd3-6f38f5a15417"), Guid.Parse("aea9bcc4-6476-44ff-9778-f4bb207abbe3"), new DateTime(2026, 9, 1, 20, 30, 0, DateTimeKind.Utc), Entities.ScreeningFormat.IMAX),  // Interstellar @ CineMax Hall 2
        (Guid.Parse("5aa88692-1b64-4000-aa2c-89e2091817e6"), Guid.Parse("09e559ff-b466-415a-81fa-103032f3f9af"), Guid.Parse("5a038fe9-45ae-447c-816a-8403076023da"), new DateTime(2026, 9, 2, 19, 0, 0, DateTimeKind.Utc), Entities.ScreeningFormat.TwoD),   // The Dark Knight @ Cineplexx Hall 1
        (Guid.Parse("9077e5b1-eb08-4640-aae6-ff0752a38575"), Guid.Parse("226b6448-bb06-439b-9b8b-8d76a73bde10"), Guid.Parse("5a038fe9-45ae-447c-816a-8403076023da"), new DateTime(2026, 9, 2, 21, 0, 0, DateTimeKind.Utc), Entities.ScreeningFormat.ThreeD), // The Godfather @ Cineplexx Hall 2
        (Guid.Parse("7de8f937-bf6b-49a2-a20b-98eaa752db74"), Guid.Parse("c80cc2fe-a9d2-41fc-9fe0-85eacb9911aa"), Guid.Parse("aea9bcc4-6476-44ff-9778-f4bb207abbe3"), new DateTime(2026, 9, 3, 22, 0, 0, DateTimeKind.Utc), Entities.ScreeningFormat.TwoD),   // The Shining @ CineMax Hall 1
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
