using MongoDB.Driver;

namespace Movie.API.Data;

public class MovieContextSeed
{
    public static async Task SeedAsync(IMongoCollection<Entities.Movie> movieCollection)
    {
        var count = await movieCollection.CountDocumentsAsync(_ => true);
        if (count > 0)
            return;

        var movies = new List<Entities.Movie>
        {
            new()
            {
                Id = Guid.Parse("ffffffff-ffff-ffff-ffff-000000000001"),
                Title = "Inception",
                Description = "A thief who steals corporate secrets through dream-sharing technology.",
                DurationMinutes = 148,
                ReleaseDate = new DateTime(2010, 7, 16),
                Rating = 8.8,
                Genres = [Genre.SciFi, Genre.Thriller, Genre.Action],
                Actors =
                [
                    new Actor { FirstName = "Leonardo", LastName = "DiCaprio" },
                    new Actor { FirstName = "Joseph", LastName = "Gordon-Levitt" }
                ],
                CoverImage = "https://resizing.flixster.com/9pwhgvMPsrGnmEtgThnX0rVzgts=/164x246/v2/https://resizing.flixster.com/-XZAfHZM39UwaGJIFWKAE8fS0ak=/v3/t/assets/p7825626_p_v10_ae.jpg"
            },
            new()
            {
                Id = Guid.Parse("ffffffff-ffff-ffff-ffff-000000000002"),
                Title = "The Godfather",
                Description = "The aging patriarch of an organized crime dynasty transfers control to his son.",
                DurationMinutes = 175,
                ReleaseDate = new DateTime(1972, 3, 24),
                Rating = 9.2,
                Genres = [Genre.Crime, Genre.Drama],
                Actors =
                [
                    new Actor { FirstName = "Marlon", LastName = "Brando" },
                    new Actor { FirstName = "Al", LastName = "Pacino" }
                ],
                CoverImage = "https://resizing.flixster.com/bUcMNsOCtzx1tV8Vp6HrpWA_Vo0=/164x246/v2/https://resizing.flixster.com/-XZAfHZM39UwaGJIFWKAE8fS0ak=/v3/t/assets/p6326_p_v12_be.jpg"
            },
            new()
            {
                Id = Guid.Parse("ffffffff-ffff-ffff-ffff-000000000003"),
                Title = "The Dark Knight",
                Description = "Batman faces the Joker, a criminal mastermind who plunges Gotham into anarchy.",
                DurationMinutes = 152,
                ReleaseDate = new DateTime(2008, 7, 18),
                Rating = 9.0,
                Genres = [Genre.Action, Genre.Crime, Genre.Drama],
                Actors =
                [
                    new Actor { FirstName = "Christian", LastName = "Bale" },
                    new Actor { FirstName = "Heath", LastName = "Ledger" }
                ],
                CoverImage = "https://resizing.flixster.com/PD15wDF15nqGushJNAW7av9-Tyk=/164x246/v2/https://resizing.flixster.com/Wg25mLoPWxjcxXzNyaSF4VGgGE4=/ems.cHJkLWVtcy1hc3NldHMvbW92aWVzL2ZiNjZiNWFkLWVhNzEtNDRhMC1iNGIwLTFmY2FkNzllNTJlMi5qcGc="
            },
            new()
            {
                Id = Guid.Parse("ffffffff-ffff-ffff-ffff-000000000004"),
                Title = "Interstellar",
                Description = "A team of explorers travel through a wormhole in space to ensure humanity's survival.",
                DurationMinutes = 169,
                ReleaseDate = new DateTime(2014, 11, 7),
                Rating = 8.6,
                Genres = [Genre.SciFi, Genre.Drama],
                Actors =
                [
                    new Actor { FirstName = "Matthew", LastName = "McConaughey" },
                    new Actor { FirstName = "Anne", LastName = "Hathaway" }
                ],
                CoverImage = "https://resizing.flixster.com/bd5A1TM4Xlur_sjlTlQux9HALic=/164x246/v2/https://resizing.flixster.com/-XZAfHZM39UwaGJIFWKAE8fS0ak=/v3/t/assets/p10543523_p_v8_as.jpg"
            },
            new()
            {
                Id = Guid.Parse("ffffffff-ffff-ffff-ffff-000000000005"),
                Title = "The Shining",
                Description = "A family heads to an isolated hotel for the winter where an evil presence influences the father.",
                DurationMinutes = 146,
                ReleaseDate = new DateTime(1980, 5, 23),
                Rating = 8.4,
                Genres = [Genre.Horror, Genre.Drama],
                Actors =
                [
                    new Actor { FirstName = "Jack", LastName = "Nicholson" },
                    new Actor { FirstName = "Shelley", LastName = "Duvall" }
                ],
                CoverImage = "https://resizing.flixster.com/Rw13WXe72pRsoe_9kr6sABSsJtQ=/164x246/v2/https://resizing.flixster.com/5E5qZzbrG8KdvlmDKFjnun2HBog=/ems.cHJkLWVtcy1hc3NldHMvbW92aWVzLzA3ZGFkNTA3LTEwZjEtNDM2Yy1hYmQxLWMyMjI4ZjY1ZjBkNy5qcGc="
            },
            new()
            {
                Id = Guid.NewGuid(),
                Title = "Three Thousand Years of Longing",
                Description = "While in Istanbul attending a conference, Dr. Alithea Binnie happens to encounter a Djinn offers her three wishes in exchange for his freedom.",
                DurationMinutes = 108,
                ReleaseDate = new DateTime(2022, 8, 26),
                Rating = 6.7,
                Genres = [Genre.Fantasy, Genre.Drama], 
                Actors = [
                    new Actor {FirstName = "Tilda", LastName = "Swinton"},
                    new Actor {FirstName = "Idris", LastName = "Elba"}
                ],
                CoverImage = "https://resizing.flixster.com/cxWZS27JxQFCqlTOtrY42B3Wy7I=/164x246/v2/https://resizing.flixster.com/6e6GoR_5a8SsCzJbKR3lO4Q1YO8=/ems.cHJkLWVtcy1hc3NldHMvbW92aWVzLzgxZDgxYTgxLWRhNGItNDJkMy1hNzNmLTg5ODAwYzg0MGRkOC5qcGc="
            }
        };

        await movieCollection.InsertManyAsync(movies);
    }
}

public static class MovieContextSeedExtensions
{
    public static async Task SeedMovieDataAsync(this IApplicationBuilder app)
    {
        using var scope = app.ApplicationServices.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<IMovieContext>();
        await MovieContextSeed.SeedAsync(context.Movies);
    }
}
