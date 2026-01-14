namespace Cinema.API.Entities;

public class Seat
{
    public Guid Id { get; set; }
    public int Row { get; set; }
    public int Number { get; set; }

    public Guid HallId { get; set; }
    public required Hall Hall { get; set; } = null!;
}
