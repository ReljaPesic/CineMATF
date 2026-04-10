namespace Cinema.API.Entities;

public class MovieTheatre
{
    public Guid Id { get; set; }
    public required string Name { get; set; }
    public City City { get; set; } = City.Beograd;

    public ICollection<Hall> Halls { get; set; } = [];
}

