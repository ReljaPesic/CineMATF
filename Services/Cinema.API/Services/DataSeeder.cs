using Cinema.API.Data;
using Cinema.API.DTOs;
using Cinema.API.Entities;
using Microsoft.EntityFrameworkCore;

namespace Cinema.API.Services;

public class DataSeeder(CinemaDbContext context, ICinemaService cinemaService)
{
    private readonly CinemaDbContext _context = context;
    private readonly ICinemaService _cinemaService = cinemaService;

    public async Task SeedAsync()
    {
        if (await _context.MovieTheatres.AnyAsync())
        {
            return;
        }

        var cinemas = new (string Name, City City, List<(string Name, int Rows, int Seats)> Halls)[]
        {
            ("CineMax", City.Beograd, [("Hall 1", 5, 10), ("Hall 2", 8, 12)]),
            ("Takvud", City.Beograd, [("Hall 1", 5, 10), ("Hall 2", 8, 12), ("Hall 3", 7, 10), ("Hall 4", 12, 10)]),
            ("Cineplexx", City.NoviSad, [("Hall 1", 6, 8), ("Hall 2", 4, 10)]),
            ("Novi Sad Cinema", City.NoviSad, [("Hall 1", 5, 10), ("Hall 2", 8, 12), ("Hall 3", 7, 10), ("Hall 4", 12, 10)]),
            ("Arena", City.Nis, [("Hall 1", 5, 10)]),
            ("Cinema City", City.Kragujevac, [("Hall 1", 4, 8), ("Hall 2", 3, 6)])
        };

        foreach (var (name, city, halls) in cinemas)
        {
            var cinemaRequest = new CinemaRequest(name, city);
            var cinema = await _cinemaService.CreateCinemaAsync(cinemaRequest);

            var hallRequests = halls.Select(h => new HallRequest(h.Name, h.Rows, h.Seats)).ToList();
            await _cinemaService.CreateHallsAsync(cinema.Id, hallRequests);
        }
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
