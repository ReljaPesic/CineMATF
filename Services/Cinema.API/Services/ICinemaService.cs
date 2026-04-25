using Cinema.API.DTOs;
using Cinema.API.Entities;

namespace Cinema.API.Services;

public interface ICinemaService
{
    Task<CinemaResponse> CreateCinemaAsync(CinemaRequest request);
    Task<PagedResponse<CinemaResponse>> GetCinemasAsync(int page, int pageSize);
    Task<IEnumerable<CinemaResponse>> GetCinemasByCityAsync(City city);
    Task<CinemaResponse?> GetCinemaByIdAsync(Guid id);
    Task<bool> DeleteCinemaAsync(Guid id);
    Task<CinemaResponse?> UpdateCinemaAsync(Guid id, CinemaRequest request);

    Task<CreateHallsResponse> CreateHallsAsync(Guid cinemaId, IEnumerable<HallRequest> requests);
    Task<IEnumerable<HallResponse>> GetHallsAsync(Guid cinemaId);
    Task<bool> DeleteHallAsync(Guid cinemaId, Guid hallId);
    Task<IEnumerable<SeatResponse>> GetSeatsAsync(Guid cinemaId, Guid hallId);
    Task<SeatResponse?> UpdateSeatTypeAsync(Guid cinemaId, Guid hallId, Guid seatId, UpdateSeatTypeRequest request);
    Task CreateSeatsAsync(Guid cinemaId, Guid hallId);

}
