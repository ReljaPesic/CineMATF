using Cinema.API.DTOs;
using Cinema.API.Entities;

namespace Cinema.API.Repositories;

public interface ICinemaRepository
{
    Task<(IEnumerable<MovieTheatre> Cinemas, int TotalCount)> GetCinemasAsync(int page, int pageSize);
    Task<MovieTheatre?> GetCinemaByIdAsync(Guid id);
    Task<MovieTheatre> CreateCinemaAsync(CinemaRequest request);
    Task<bool> DeleteCinemaAsync(Guid id);
    Task<bool> UpdateCinemaAsync(MovieTheatre newCinema);

    Task<IEnumerable<Hall>> GetHallsAsync(Guid cinemaId);
    Task<Hall?> GetHallByIdAsync(Guid hallId, Guid cinemaId);
    Task<Hall> CreateHallAsync(Guid cinemaId, HallRequest hall);
    Task<bool> DeleteHallAsync(Guid cinemaId, Guid hallId);
    Task<bool> UpdateHallAsync(Hall newHall);

    Task<IEnumerable<Seat>> GetSeatLayoutAsync(Guid hallId);
    Task<Seat?> GetSeatByIdAsync(Guid seatId);
    Task<bool> UpdateSeatAsync(Seat seat);
    Task CreateSeatsAsync(Guid hallId);
    Task CreateSeatsAsync(Guid hallId, IEnumerable<Seat> seats);
}
