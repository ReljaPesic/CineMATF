using Entities = Screening.API.Entities;

namespace Screening.API.Repositories;

public interface IScreeningRepository
{
    Task<IEnumerable<Entities.Screening>> GetScreeningsAsync(Guid? movieId, DateOnly? date, Guid? cinemaId, IReadOnlyCollection<Guid>? restrictToCinemaIds);
    Task<Entities.Screening?> GetScreeningByIdAsync(Guid id);
    Task<IEnumerable<Entities.Screening>> GetScreeningsByHallAsync(Guid hallId);
    Task<Entities.Screening> CreateScreeningAsync(Entities.Screening screening);
    Task<bool> UpdateScreeningAsync(Entities.Screening screening);
    Task<bool> DeleteScreeningAsync(Guid id);
}
