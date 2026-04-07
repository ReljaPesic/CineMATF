using AutoMapper;
using Cinema.API.DTOs;
using Cinema.API.Entities;
using Cinema.API.Repositories;

namespace Cinema.API.Services;

public class CinemaService(ICinemaRepository repository, IMapper mapper) : ICinemaService
{
    public async Task<CinemaResponse> CreateCinemaAsync(CinemaRequest request)
    {
        var cinema = await repository.CreateCinemaAsync(request);
        return mapper.Map<CinemaResponse>(cinema);
    }

    public async Task<(IEnumerable<CinemaResponse> Cinemas, int TotalCount)> GetCinemasAsync(int page, int pageSize)
    {
        var (cinemas, totalCount) = await repository.GetCinemasAsync(page, pageSize);
        return (mapper.Map<IEnumerable<CinemaResponse>>(cinemas), totalCount);
    }

    public async Task<CinemaResponse?> GetCinemaByIdAsync(Guid id)
    {
        var cinema = await repository.GetCinemaByIdAsync(id);
        return cinema == null ? null : mapper.Map<CinemaResponse>(cinema);
    }

    public async Task<bool> DeleteCinemaAsync(Guid id)
    {
        return await repository.DeleteCinemaAsync(id);
    }

    public async Task<CinemaResponse?> UpdateCinemaAsync(Guid id, CinemaRequest request)
    {
        var existing = await repository.GetCinemaByIdAsync(id);
        if (existing == null) return null;

        existing.Name = request.Name;
        existing.City = request.City;

        await repository.UpdateCinemaAsync(existing);
        return mapper.Map<CinemaResponse>(existing);
    }

    public async Task<HallResponse> CreateHallAsync(Guid cinemaId, HallRequest request)
    {
        var hall = await repository.CreateHallAsync(cinemaId, request);
        hall.InitializeSeats();
        await repository.CreateSeatsAsync(hall.Id, hall.Seats);
        return mapper.Map<HallResponse>(hall);
    }

    public async Task<int> CreateHallsAsync(Guid cinemaId, IEnumerable<HallRequest> requests)
    {
        int count = 0;
        foreach (var request in requests)
        {
            try
            {
                var hall = await repository.CreateHallAsync(cinemaId, request);
                hall.InitializeSeats();
                await repository.CreateSeatsAsync(hall.Id, hall.Seats);
                count++;
            }
            catch
            {
                // Skip on error, continue with next
            }
        }
        return count;
    }

    public async Task<IEnumerable<HallResponse>> GetHallsAsync(Guid cinemaId)
    {
        var halls = await repository.GetHallsAsync(cinemaId);
        return mapper.Map<IEnumerable<HallResponse>>(halls);
    }

    public async Task<bool> DeleteHallAsync(Guid cinemaId, Guid hallId)
    {
        return await repository.DeleteHallAsync(cinemaId, hallId);
    }

    public async Task<IEnumerable<SeatResponse>> GetSeatsAsync(Guid hallId)
    {
        var seats = await repository.GetSeatLayoutAsync(hallId);
        return mapper.Map<IEnumerable<SeatResponse>>(seats);
    }

    public async Task<SeatResponse?> UpdateSeatTypeAsync(Guid seatId, UpdateSeatTypeRequest request)
    {
        var seat = await repository.GetSeatByIdAsync(seatId);
        if (seat == null) return null;

        if (!Enum.TryParse<SeatType>(request.SeatType, true, out var seatType))
            return null;

        seat.SeatType = seatType;
        await repository.UpdateSeatAsync(seat);
        return mapper.Map<SeatResponse>(seat);
    }

    public async Task CreateSeatsAsync(Guid hallId)
    {
        await repository.CreateSeatsAsync(hallId);
    }
}
