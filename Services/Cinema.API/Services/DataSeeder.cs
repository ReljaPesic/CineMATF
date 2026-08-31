using Cinema.API.Data;
using Cinema.API.Entities;
using Microsoft.EntityFrameworkCore;

namespace Cinema.API.Services;

public class DataSeeder(CinemaDbContext context)
{
    private readonly CinemaDbContext _context = context;

    private static readonly (Guid Id, string Name, City City, (Guid Id, string Name, int Rows, int Seats)[] Halls)[] Cinemas =
    [
        (Guid.Parse("cccccccc-cccc-cccc-cccc-000000000001"), "CineMax", City.Beograd,
        [
            (Guid.Parse("bbbbbbbb-bbbb-bbbb-0001-000000000001"), "Hall 1", 5, 10),
            (Guid.Parse("bbbbbbbb-bbbb-bbbb-0001-000000000002"), "Hall 2", 8, 12)
        ]),
        (Guid.Parse("cccccccc-cccc-cccc-cccc-000000000002"), "Takvud", City.Beograd,
        [
            (Guid.Parse("bbbbbbbb-bbbb-bbbb-0002-000000000001"), "Hall 1", 5, 10),
            (Guid.Parse("bbbbbbbb-bbbb-bbbb-0002-000000000002"), "Hall 2", 8, 12),
            (Guid.Parse("bbbbbbbb-bbbb-bbbb-0002-000000000003"), "Hall 3", 7, 10),
            (Guid.Parse("bbbbbbbb-bbbb-bbbb-0002-000000000004"), "Hall 4", 12, 10)
        ]),
        (Guid.Parse("cccccccc-cccc-cccc-cccc-000000000003"), "Cineplexx", City.NoviSad,
        [
            (Guid.Parse("bbbbbbbb-bbbb-bbbb-0003-000000000001"), "Hall 1", 6, 8),
            (Guid.Parse("bbbbbbbb-bbbb-bbbb-0003-000000000002"), "Hall 2", 4, 10)
        ]),
        (Guid.Parse("cccccccc-cccc-cccc-cccc-000000000004"), "Novi Sad Cinema", City.NoviSad,
        [
            (Guid.Parse("bbbbbbbb-bbbb-bbbb-0004-000000000001"), "Hall 1", 5, 10),
            (Guid.Parse("bbbbbbbb-bbbb-bbbb-0004-000000000002"), "Hall 2", 8, 12),
            (Guid.Parse("bbbbbbbb-bbbb-bbbb-0004-000000000003"), "Hall 3", 7, 10),
            (Guid.Parse("bbbbbbbb-bbbb-bbbb-0004-000000000004"), "Hall 4", 12, 10)
        ]),
        (Guid.Parse("cccccccc-cccc-cccc-cccc-000000000005"), "Arena", City.Nis,
        [
            (Guid.Parse("bbbbbbbb-bbbb-bbbb-0005-000000000001"), "Hall 1", 5, 10)
        ]),
        (Guid.Parse("cccccccc-cccc-cccc-cccc-000000000006"), "Cinema City", City.Kragujevac,
        [
            (Guid.Parse("bbbbbbbb-bbbb-bbbb-0006-000000000001"), "Hall 1", 4, 8),
            (Guid.Parse("bbbbbbbb-bbbb-bbbb-0006-000000000002"), "Hall 2", 3, 6)
        ])
    ];

    public async Task SeedAsync()
    {
        if (await _context.MovieTheatres.AnyAsync())
        {
            return;
        }

        foreach (var (cinemaId, name, city, halls) in Cinemas)
        {
            var cinema = new MovieTheatre { Id = cinemaId, Name = name, City = city };
            _context.MovieTheatres.Add(cinema);

            foreach (var (hallId, hallName, rows, seatsPerRow) in halls)
            {
                var hall = new Hall { Id = hallId, Name = hallName, TotalRows = rows, SeatsPerRow = seatsPerRow, CinemaId = cinemaId };
                hall.InitializeSeats();
                _context.Halls.Add(hall);
                _context.Seats.AddRange(hall.Seats);
            }
        }

        await _context.SaveChangesAsync();
    }
}

public static class DataSeederExtensions
{
    public static async Task SeedDataAsync(this IApplicationBuilder app)
    {
        using var scope = app.ApplicationServices.CreateScope();
        var seeder = scope.ServiceProvider.GetRequiredService<DataSeeder>();
        await seeder.SeedAsync();
    }
}
