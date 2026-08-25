using MongoDB.Driver;
using Movie.API.Data;
using Movie.API.Entities;

namespace Movie.API.Repositories;

public class MovieRepository(IMovieContext context) : IMovieRepository
{
    private readonly IMovieContext _context = context ?? throw new ArgumentNullException(nameof(context));

    public async Task<(IEnumerable<Entities.Movie> Movies, int TotalCount)> GetMoviesAsync(int page, int pageSize)
    {
        var totalCount = (int)await _context.Movies.CountDocumentsAsync(_ => true);
        var movies = await _context.Movies.Find(_ => true)
            .SortBy(m => m.Title)
            .Skip((page - 1) * pageSize)
            .Limit(pageSize)
            .ToListAsync();

        return (movies, totalCount);
    }

    public async Task<Entities.Movie?> GetMovieByIdAsync(Guid id)
    {
        return await _context.Movies.Find(m => m.Id == id).FirstOrDefaultAsync();
    }

    public async Task<Entities.Movie> CreateMovieAsync(Entities.Movie movie)
    {
        movie.Id = Guid.NewGuid();
        await _context.Movies.InsertOneAsync(movie);
        return movie;
    }

    public async Task<IEnumerable<Entities.Movie>> GetMoviesByGenreAsync(Genre genre)
    {
        return await _context.Movies.Find(m => m.Genres.Contains(genre)).ToListAsync();
    }

    public async Task<IEnumerable<Entities.Movie>> GetMoviesByTitleAsync(string title)
    {
        var filter = Builders<Entities.Movie>.Filter.Regex(m => m.Title, new MongoDB.Bson.BsonRegularExpression(title, "i"));
        return await _context.Movies.Find(filter).ToListAsync();
    }

    public async Task<bool> UpdateMovieAsync(Entities.Movie movie)
    {
        var result = await context.Movies.ReplaceOneAsync(m => m.Id == movie.Id, movie);
        return result.IsAcknowledged && result.MatchedCount > 0;
    }

    public async Task<bool> DeleteMovieAsync(Guid id)
    {
        var result = await _context.Movies.DeleteOneAsync(m => m.Id == id);
        return result.IsAcknowledged && result.DeletedCount > 0;
    }
}
