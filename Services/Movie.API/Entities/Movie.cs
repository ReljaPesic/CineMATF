namespace Movie.API.Entities;
public class Movie
{
    public Guid Id { get; set; }
    public required string Title { get; set; }
    public required string Description { get; set; }
    public int DurationMinutes { get; set; }
    public DateTime ReleaseDate { get; set; }
    public double Rating { get; set; }
    public required List<Actor> Actors { get; set; }
    public required List<Genre> Genres { get; set; }
    public string? CoverImage { get; set; }
}