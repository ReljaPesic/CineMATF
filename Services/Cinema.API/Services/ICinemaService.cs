using Cinema.API.DTOs;
using Cinema.API.Entities;

namespace Cinema.API.Services;

public interface ICinemaService
{
    Task<CinemaResponse> CreateCinemaAsync(CinemaRequest request);
    Task<(IEnumerable<CinemaResponse> Cinemas, int TotalCount)> GetCinemasAsync(int page, int pageSize);
    Task<CinemaResponse?> GetCinemaByIdAsync(Guid id);
    Task<bool> DeleteCinemaAsync(Guid id);
    Task<CinemaResponse?> UpdateCinemaAsync(Guid id, CinemaRequest request);

    Task<HallResponse> CreateHallAsync(Guid cinemaId, HallRequest request);
    Task<int> CreateHallsAsync(Guid cinemaId, IEnumerable<HallRequest> requests);
    Task<IEnumerable<HallResponse>> GetHallsAsync(Guid cinemaId);
    Task<bool> DeleteHallAsync(Guid cinemaId, Guid hallId);
    Task<IEnumerable<Seat>> GetSeatsAsync(Guid hallId);
    Task CreateSeatsAsync(Guid hallId);

}
