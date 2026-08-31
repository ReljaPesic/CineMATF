using Screening.API.DTOs;

namespace Screening.API.Services;

public interface IScreeningService
{
    Task<IEnumerable<ScreeningResponse>> GetScreeningsAsync(Guid? movieId, DateOnly? date, Guid? cinemaId);
    Task<ScreeningResponse?> GetScreeningByIdAsync(Guid id);
    Task<ScreeningResponse> CreateScreeningAsync(ScreeningRequest request);
    Task<ScreeningResponse?> UpdateScreeningAsync(Guid id, ScreeningRequest request);
    Task<bool> DeleteScreeningAsync(Guid id);
}
