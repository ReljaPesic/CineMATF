using Screening.API.DTOs;

namespace Screening.API.Services;

public interface IScreeningService
{
    Task<IEnumerable<ScreeningResponse>> GetScreeningsAsync(Guid? movieId, DateOnly? date, Guid? cinemaId, IReadOnlyCollection<Guid>? restrictToCinemaIds);
    Task<ScreeningResponse?> GetScreeningByIdAsync(Guid id);
    Task<(bool Success, string? ErrorMessage, ScreeningResponse? Response)> CreateScreeningAsync(ScreeningRequest request);
    Task<(bool Success, string? ErrorMessage, ScreeningResponse? Response)> UpdateScreeningAsync(Guid id, ScreeningRequest request);
    Task<bool> DeleteScreeningAsync(Guid id);
}
