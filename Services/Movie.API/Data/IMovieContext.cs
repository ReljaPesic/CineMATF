using MongoDB.Driver;

namespace Movie.API.Data;

public interface IMovieContext
{
    IMongoCollection<Entities.Movie> Movies { get; }
}
