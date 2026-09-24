using AutoMapper;
using Screening.API.DTOs;
using Screening.API.ExternalServices;
using Screening.API.Repositories;
using Entities = Screening.API.Entities;

namespace Screening.API.Services;

public class ScreeningService(IScreeningRepository repository, IMapper mapper, IMovieApiClient movieApiClient) : IScreeningService
{
    // On top of the movie's own runtime, this covers cleaning/exit time before the hall
    // is free for the next screening.
    private const int OccupiedBufferMinutes = 30;

    public async Task<IEnumerable<ScreeningResponse>> GetScreeningsAsync(Guid? movieId, DateOnly? date, Guid? cinemaId, IReadOnlyCollection<Guid>? restrictToCinemaIds)
    {
        var screenings = await repository.GetScreeningsAsync(movieId, date, cinemaId, restrictToCinemaIds);
        return mapper.Map<IEnumerable<ScreeningResponse>>(screenings);
    }

    public async Task<ScreeningResponse?> GetScreeningByIdAsync(Guid id)
    {
        var screening = await repository.GetScreeningByIdAsync(id);
        return screening == null ? null : mapper.Map<ScreeningResponse>(screening);
    }

    public async Task<(bool Success, string? ErrorMessage, ScreeningResponse? Response)> CreateScreeningAsync(ScreeningRequest request)
    {
        var conflict = await FindConflictAsync(request.HallId, request.MovieId, request.StartTime, excludeScreeningId: null);
        if (conflict != null) return (false, conflict, null);

        var screening = mapper.Map<Entities.Screening>(request);
        var created = await repository.CreateScreeningAsync(screening);
        return (true, null, mapper.Map<ScreeningResponse>(created));
    }

    public async Task<(bool Success, string? ErrorMessage, ScreeningResponse? Response)> UpdateScreeningAsync(Guid id, ScreeningRequest request)
    {
        var existing = await repository.GetScreeningByIdAsync(id);
        if (existing == null) return (false, "Screening not found", null);

        var conflict = await FindConflictAsync(request.HallId, request.MovieId, request.StartTime, excludeScreeningId: id);
        if (conflict != null) return (false, conflict, null);

        var screening = mapper.Map<Entities.Screening>(request);
        screening.Id = id;

        var updated = await repository.UpdateScreeningAsync(screening);
        return updated ? (true, null, mapper.Map<ScreeningResponse>(screening)) : (false, "Screening not found", null);
    }

    public async Task<bool> DeleteScreeningAsync(Guid id)
    {
        return await repository.DeleteScreeningAsync(id);
    }

    // A screening occupies its hall from its start time through the movie's runtime plus
    // OccupiedBufferMinutes. Two screenings in the same hall conflict if those windows overlap.
    private async Task<string?> FindConflictAsync(Guid hallId, Guid movieId, DateTime startTime, Guid? excludeScreeningId)
    {
        var movie = await movieApiClient.GetMovieAsync(movieId);
        if (movie == null) return "Movie not found";

        var newEnd = startTime.AddMinutes(movie.DurationMinutes + OccupiedBufferMinutes);

        var existingScreenings = await repository.GetScreeningsByHallAsync(hallId);
        foreach (var existing in existingScreenings)
        {
            if (excludeScreeningId != null && existing.Id == excludeScreeningId) continue;

            var existingMovie = await movieApiClient.GetMovieAsync(existing.MovieId);
            var existingEnd = existing.StartTime.AddMinutes((existingMovie?.DurationMinutes ?? 0) + OccupiedBufferMinutes);

            var overlaps = startTime < existingEnd && existing.StartTime < newEnd;
            if (overlaps)
            {
                return $"Hall is already occupied from {existing.StartTime:yyyy-MM-dd HH:mm} to {existingEnd:HH:mm} by another screening.";
            }
        }

        return null;
    }
}
