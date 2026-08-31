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

    public async Task<PagedResponse<CinemaResponse>> GetCinemasAsync(int page, int pageSize)
    {
        var (cinemas, totalCount) = await repository.GetCinemasAsync(page, pageSize);
        return new PagedResponse<CinemaResponse>(
            mapper.Map<IEnumerable<CinemaResponse>>(cinemas),
            page,
            pageSize,
            totalCount
        );
    }

    public async Task<IEnumerable<CinemaResponse>> GetCinemasByCityAsync(City city)
    {
        var cinemas = await repository.GetCinemasByCityAsync(city);
        return mapper.Map<IEnumerable<CinemaResponse>>(cinemas);
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

    public async Task<CreateHallsResponse> CreateHallsAsync(Guid cinemaId, IEnumerable<HallRequest> requests)
    {
        int count = 0;
        var failed = new List<FailedHall>();

        foreach (var request in requests)
        {
            try
            {
                var hall = await repository.CreateHallAsync(cinemaId, request);
                hall.InitializeSeats();
                await repository.CreateSeatsAsync(hall.Id, hall.Seats);
                count++;
            }
            catch (Exception)
            {
                failed.Add(new FailedHall(request.Name, "duplicate"));
            }
        }

        return new CreateHallsResponse(count, failed);
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

    public async Task<IEnumerable<SeatResponse>> GetSeatsAsync(Guid cinemaId, Guid hallId)
    {
        var hall = await repository.GetHallByIdAsync(hallId, cinemaId) ?? throw new KeyNotFoundException($"Hall with ID {hallId} not found in cinema {cinemaId}");
        var seats = await repository.GetSeatLayoutAsync(hall.Id);
        return mapper.Map<IEnumerable<SeatResponse>>(seats);
    }

    public async Task<SeatResponse?> GetSeatByIdAsync(Guid seatId)
    {
        var seat = await repository.GetSeatByIdAsync(seatId);
        return seat == null ? null : mapper.Map<SeatResponse>(seat);
    }

    public async Task<SeatResponse?> UpdateSeatTypeAsync(Guid cinemaId, Guid hallId, Guid seatId, UpdateSeatTypeRequest request)
    {
        var hall = await repository.GetHallByIdAsync(hallId, cinemaId) ?? throw new KeyNotFoundException($"Hall with ID {hallId} not found in cinema {cinemaId}");
        var seat = await repository.GetSeatByIdAsync(seatId);
        if (seat == null || seat.HallId != hall.Id)
            throw new KeyNotFoundException($"Seat with ID {seatId} not found in hall {hallId}");

        seat.SeatType = request.SeatType;
        await repository.UpdateSeatAsync(seat);
        return mapper.Map<SeatResponse>(seat);
    }

    public async Task CreateSeatsAsync(Guid cinemaId, Guid hallId)
    {
        var hall = await repository.GetHallByIdAsync(hallId, cinemaId) ?? throw new KeyNotFoundException($"Hall with ID {hallId} not found in cinema {cinemaId}");
        await repository.CreateSeatsAsync(hall.Id);
    }
}
