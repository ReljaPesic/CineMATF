using AutoMapper;
using Screening.API.DTOs;
using Screening.API.Repositories;
using Entities = Screening.API.Entities;

namespace Screening.API.Services;

public class ScreeningService(IScreeningRepository repository, IMapper mapper) : IScreeningService
{
    public async Task<IEnumerable<ScreeningResponse>> GetScreeningsAsync(Guid? movieId, DateOnly? date, Guid? cinemaId)
    {
        var screenings = await repository.GetScreeningsAsync(movieId, date, cinemaId);
        return mapper.Map<IEnumerable<ScreeningResponse>>(screenings);
    }

    public async Task<ScreeningResponse?> GetScreeningByIdAsync(Guid id)
    {
        var screening = await repository.GetScreeningByIdAsync(id);
        return screening == null ? null : mapper.Map<ScreeningResponse>(screening);
    }

    public async Task<ScreeningResponse> CreateScreeningAsync(ScreeningRequest request)
    {
        var screening = mapper.Map<Entities.Screening>(request);
        var created = await repository.CreateScreeningAsync(screening);
        return mapper.Map<ScreeningResponse>(created);
    }

    public async Task<ScreeningResponse?> UpdateScreeningAsync(Guid id, ScreeningRequest request)
    {
        var existing = await repository.GetScreeningByIdAsync(id);
        if (existing == null) return null;

        var screening = mapper.Map<Entities.Screening>(request);
        screening.Id = id;
        screening.Status = existing.Status;

        var updated = await repository.UpdateScreeningAsync(screening);
        return updated ? mapper.Map<ScreeningResponse>(screening) : null;
    }

    public async Task<bool> DeleteScreeningAsync(Guid id)
    {
        return await repository.DeleteScreeningAsync(id);
    }
}
