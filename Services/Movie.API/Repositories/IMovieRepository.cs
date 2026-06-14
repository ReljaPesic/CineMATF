using Movie.API.Entities;

namespace Movie.API.Repositories;

public interface IMovieRepository
{
    Task<IEnumerable<Entities.Movie>> GetMoviesAsync();
    Task<Entities.Movie?> GetMovieByIdAsync(Guid id);
    Task<IEnumerable<Entities.Movie>> GetMoviesByGenreAsync(Genre genre);
    Task<IEnumerable<Entities.Movie>> GetMoviesByTitleAsync(string title);
    Task<Entities.Movie> CreateMovieAsync(Entities.Movie movie);
    Task<bool> UpdateMovieAsync(Entities.Movie movie);
    Task<bool> DeleteMovieAsync(Guid id);
}
