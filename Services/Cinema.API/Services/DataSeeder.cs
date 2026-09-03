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
                AssignSeatTypes(hall);
                _context.Halls.Add(hall);
                _context.Seats.AddRange(hall.Seats);
            }
        }

        await _context.SaveChangesAsync();
    }

    // InitializeSeats() only lays out plain Standard seats - that's the right default for a
    // freshly-generated hall an admin will customize by hand. Seed data instead assigns a
    // realistic mix upfront so every seat type (and its pricing) is visible without manual setup.
    private static void AssignSeatTypes(Hall hall)
    {
        var byRow = hall.Seats.GroupBy(s => s.Row).ToDictionary(g => g.Key, g => g.OrderBy(s => s.Number).ToList());

        // Back row: premium VIP seats.
        foreach (var seat in byRow[hall.TotalRows - 1])
        {
            seat.SeatType = SeatType.VIP;
        }

        // Second-to-last row: a couple seat pair at each end, if the row is wide enough.
        if (hall.TotalRows >= 2 && byRow[hall.TotalRows - 2].Count >= 4)
        {
            var row = byRow[hall.TotalRows - 2];
            row[0].SeatType = SeatType.Couple;
            row[1].SeatType = SeatType.Couple;
            row[^2].SeatType = SeatType.Couple;
            row[^1].SeatType = SeatType.Couple;
        }

        // Front row: accessible seats on the aisle.
        var frontRow = byRow[0];
        frontRow[0].SeatType = SeatType.Accessible;
        frontRow[^1].SeatType = SeatType.Accessible;
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
