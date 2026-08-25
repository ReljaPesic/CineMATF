using AutoMapper;
using Movie.API.DTOs;
using Movie.API.Entities;
using Movie.API.Repositories;

namespace Movie.API.Services;

public class MovieService(IMovieRepository repository, IMapper mapper) : IMovieService
{
    public async Task<PagedResponse<MovieResponse>> GetMoviesAsync(int page, int pageSize)
    {
        var (movies, totalCount) = await repository.GetMoviesAsync(page, pageSize);
        return new PagedResponse<MovieResponse>(
            mapper.Map<IEnumerable<MovieResponse>>(movies),
            page,
            pageSize,
            totalCount
        );
    }

    public async Task<MovieResponse?> GetMovieByIdAsync(Guid id)
    {
        var movie = await repository.GetMovieByIdAsync(id);
        return movie == null ? null : mapper.Map<MovieResponse>(movie);
    }

    public async Task<IEnumerable<MovieResponse>> GetMoviesByGenreAsync(Genre genre)
    {
        var movies = await repository.GetMoviesByGenreAsync(genre);
        return mapper.Map<IEnumerable<MovieResponse>>(movies);
    }

    public async Task<IEnumerable<MovieResponse>> GetMoviesByTitleAsync(string title)
    {
        var movies = await repository.GetMoviesByTitleAsync(title);
        return mapper.Map<IEnumerable<MovieResponse>>(movies);
    }

    public async Task<MovieResponse> CreateMovieAsync(MovieRequest request)
    {
        var movie = mapper.Map<Entities.Movie>(request);
        var created = await repository.CreateMovieAsync(movie);
        return mapper.Map<MovieResponse>(created);
    }

    public async Task<MovieResponse?> UpdateMovieAsync(Guid id, MovieRequest request)
    {
        var existing = await repository.GetMovieByIdAsync(id);
        if (existing == null) return null;

        var movie = mapper.Map<Entities.Movie>(request);
        movie.Id = id;

        var updated = await repository.UpdateMovieAsync(movie);
        return updated ? mapper.Map<MovieResponse>(movie) : null;
    }

    public async Task<bool> DeleteMovieAsync(Guid id)
    {
        return await repository.DeleteMovieAsync(id);
    }
}
