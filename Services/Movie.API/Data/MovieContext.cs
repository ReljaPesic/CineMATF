using MongoDB.Driver;

namespace Movie.API.Data;

public class MovieContext : IMovieContext
{
    public MovieContext(IConfiguration configuration)
    {
        var client = new MongoClient(configuration.GetValue<string>("DatabaseSettings:ConnectionString"));
        var database = client.GetDatabase(configuration.GetValue<string>("DatabaseSettings:DatabaseName"));
        Movies = database.GetCollection<Entities.Movie>(configuration.GetValue<string>("DatabaseSettings:CollectionName"));
        MovieContextSeed.SeedAsync(Movies).Wait();
    }

    public IMongoCollection<Entities.Movie> Movies { get; }
}
