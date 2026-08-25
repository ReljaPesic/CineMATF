using Movie.API.DTOs;
using Movie.API.Entities;

namespace Movie.API.Services;

public interface IMovieService
{
    Task<PagedResponse<MovieResponse>> GetMoviesAsync(int page, int pageSize);
    Task<MovieResponse?> GetMovieByIdAsync(Guid id);
    Task<IEnumerable<MovieResponse>> GetMoviesByGenreAsync(Genre genre);
    Task<IEnumerable<MovieResponse>> GetMoviesByTitleAsync(string title);
    Task<MovieResponse> CreateMovieAsync(MovieRequest request);
    Task<MovieResponse?> UpdateMovieAsync(Guid id, MovieRequest request);
    Task<bool> DeleteMovieAsync(Guid id);
}
