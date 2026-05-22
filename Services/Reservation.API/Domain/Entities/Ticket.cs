namespace Reservation.API.Domain.Entities;

public class Ticket
{
    public Guid Id { get; set; }
    public Guid ReservationId { get; set; }
    public Guid SeatId { get; set; }
    public int SeatRow { get; set; }
    public int SeatNumber { get; set; }
    public decimal Price { get; set; }
    public string QrCode { get; set; } = string.Empty;

    public Reservation? Reservation { get; set; }
}