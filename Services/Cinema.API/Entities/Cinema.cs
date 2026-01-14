namespace Cinema.API.Entities;

public class Cinema
{
    public Guid Id { get; set; }
    public required string Name { get; set; }
    public required string City { get; set; }

    public ICollection<Hall> Projections { get; set; } = [];
}

