namespace Cinema.API.Entities;

public class MovieTheatre
{
    public Guid Id { get; set; }
    public required string Name { get; set; }
    public required string City { get; set; }

    public ICollection<Hall> Halls { get; set; } = [];
}

