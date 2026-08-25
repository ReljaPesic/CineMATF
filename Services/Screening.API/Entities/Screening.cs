namespace Screening.API.Entities;

public class Screening
{
    public Guid Id { get; set; }
    public Guid MovieId { get; set; }
    public Guid HallId { get; set; }
    public Guid CinemaId { get; set; }
    public DateTime StartTime { get; set; }
    public ScreeningFormat Format { get; set; } = ScreeningFormat.TwoD;
    public ScreeningStatus Status { get; set; } = ScreeningStatus.Available;
}
