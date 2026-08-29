using Entities = Screening.API.Entities;

namespace Screening.API.Repositories;

public interface IScreeningRepository
{
    Task<IEnumerable<Entities.Screening>> GetScreeningsAsync(Guid? movieId, DateOnly? date, Guid? cinemaId);
    Task<Entities.Screening?> GetScreeningByIdAsync(Guid id);
    Task<Entities.Screening> CreateScreeningAsync(Entities.Screening screening);
    Task<bool> UpdateScreeningAsync(Entities.Screening screening);
    Task<bool> DeleteScreeningAsync(Guid id);
}
