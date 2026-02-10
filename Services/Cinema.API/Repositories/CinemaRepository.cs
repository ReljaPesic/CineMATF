using Cinema.API.Data;
using Cinema.API.Entities;
using Microsoft.EntityFrameworkCore;

namespace Cinema.API.Repositories;

public class CinemaRepository(CinemaDbContext context) : ICinemaRepository
{
    private readonly CinemaDbContext _context = context ?? throw new ArgumentNullException(nameof(context));

    public async Task CreateCinemaAsync(MovieTheatre cinema)
    {
        await _context.MovieTheatres.AddAsync(cinema);
        await _context.SaveChangesAsync();
    }

    public async Task CreateHallAsync(Guid cinemaId, Hall hall)
    {
        var cinema = await _context.MovieTheatres.FindAsync(cinemaId);
        if (cinema == null) return;

        hall.CinemaId = cinemaId;

        await _context.Halls.AddAsync(hall);
        await _context.SaveChangesAsync();
    }

    public async Task CreateSeatsAsync(Guid hallId)
    {
        var hall = await _context.Halls.FindAsync(hallId);
        if (hall == null) return;

        hall.InitializeSeats();

        await _context.Seats.AddRangeAsync(hall.Seats);
        await _context.SaveChangesAsync();
    }

    public async Task<bool> DeleteCinemaAsync(Guid id)
    {
        var cinema = await _context.MovieTheatres.Include(c => c.Halls).ThenInclude(h => h.Seats).FirstOrDefaultAsync(c => c.Id == id);

        if (cinema == null) return false;

        _context.MovieTheatres.Remove(cinema);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteHallAsync(Guid cinemaId, Guid hallId)
    {
        var hall = await _context.Halls.Include(h => h.Seats).FirstOrDefaultAsync(h => h.Id == hallId && h.CinemaId == cinemaId);
        if (hall == null) return false;

        _context.Halls.Remove(hall);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<MovieTheatre?> GetCinemaByIdAsync(Guid id)
    {
        return await _context.MovieTheatres.Include(c => c.Halls).ThenInclude(h => h.Seats).FirstOrDefaultAsync(c => c.Id == id);
    }

    public async Task<IEnumerable<MovieTheatre>> GetCinemasAsync()
    {
        return await _context.MovieTheatres.Include(c => c.Halls).ThenInclude(h => h.Seats).ToListAsync();
    }

    public async Task<IEnumerable<Hall>> GetHallsAsync(Guid cinemaId)
    {
        return await _context.Halls.Where(h => h.CinemaId == cinemaId).Include(h => h.Seats).ToListAsync();
    }

    public async Task<IEnumerable<Seat>> GetSeatLayoutAsync(Guid hallId)
    {
        return await _context.Seats.Where(s => s.HallId == hallId).ToListAsync();
    }

    public async Task<bool> UpdateCinemaAsync(MovieTheatre newCinema)
    {
        var exists = await _context.MovieTheatres.AnyAsync(c => c.Id == newCinema.Id);
        if (!exists) return false;

        _context.MovieTheatres.Update(newCinema);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> UpdateHallAsync(Hall newHall)
    {
        var exists = await _context.Halls.AnyAsync(h => h.Id == newHall.Id && h.CinemaId == newHall.CinemaId);
        if (!exists) return false;

        _context.Halls.Update(newHall);
        await _context.SaveChangesAsync();
        return true;
    }
}
