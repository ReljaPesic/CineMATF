using Cinema.API.Data;
using Cinema.API.DTOs;
using Cinema.API.Entities;
using Microsoft.EntityFrameworkCore;

namespace Cinema.API.Repositories;

public class CinemaRepository(CinemaDbContext context) : ICinemaRepository
{
    private readonly CinemaDbContext _context = context ?? throw new ArgumentNullException(nameof(context));

    public async Task<MovieTheatre> CreateCinemaAsync(CinemaRequest request)
    {
        var cinema = new MovieTheatre
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            City = request.City
        };
        _context.MovieTheatres.Add(cinema);
        await _context.SaveChangesAsync();
        return cinema;
    }

    public async Task<Hall> CreateHallAsync(Guid cinemaId, HallRequest request)
    {
        var cinema = await _context.MovieTheatres.FindAsync(cinemaId) ?? throw new KeyNotFoundException($"Cinema with ID {cinemaId} not found");
        var hall = new Hall
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            TotalRows = request.TotalRows,
            SeatsPerRow = request.SeatsPerRow,
            CinemaId = cinema.Id
        };

        await _context.Halls.AddAsync(hall);
        await _context.SaveChangesAsync();
        return hall;
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
        return await _context.MovieTheatres.AsNoTracking().Include(c => c.Halls).FirstOrDefaultAsync(c => c.Id == id);
    }

    public async Task<(IEnumerable<MovieTheatre> Cinemas, int TotalCount)> GetCinemasAsync(int page, int pageSize)
    {
        var query = _context.MovieTheatres.AsNoTracking();
        var totalCount = await query.CountAsync();
        var items = await query
            .OrderBy(c => c.Name)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, totalCount);
    }

    public async Task<IEnumerable<Hall>> GetHallsAsync(Guid cinemaId)
    {
        return await _context.Halls.Where(h => h.CinemaId == cinemaId).AsNoTracking().ToListAsync();
    }

    public async Task<IEnumerable<Seat>> GetSeatLayoutAsync(Guid hallId)
    {
        return await _context.Seats.AsNoTracking().Where(s => s.HallId == hallId).OrderBy(s => s.Row).ThenBy(s => s.Number).ToListAsync();
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
        var existing = await _context.Halls
            .FirstOrDefaultAsync(h => h.Id == newHall.Id && h.CinemaId == newHall.CinemaId);

        if (existing == null) return false;
        existing.Name = newHall.Name;
        existing.TotalRows = newHall.TotalRows;
        existing.SeatsPerRow = newHall.SeatsPerRow;

        await _context.SaveChangesAsync();
        return true;
    }
}
