namespace Cinema.API.Entities;

public class Hall
{
    public Guid Id { get; set; }
    public required string Name { get; set; }
    public int TotalRows { get; set; }
    public int SeatsPerRow { get; set; }

    public Guid CinemaId { get; set; }
    public MovieTheatre? Cinema { get; set; }

    public ICollection<Seat> Seats { get; set; } = [];

    public void InitializeSeats()
    {
        for (int i = 0; i < TotalRows; i++)
        {
            for (int j = 0; j < SeatsPerRow; j++)
            {
                Seats.Add(new Seat
                {
                    Row = i,
                    Number = j,
                    Hall = this
                });
            }
        }
    }
}
