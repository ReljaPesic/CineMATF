using Reservation.API.Domain.Enums;

namespace Reservation.API.Domain.Entities;

public class Reservation
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public Guid ScreeningId { get; set; }
    public ReservationStatus Status { get; set; } = ReservationStatus.Locked;
    public decimal TotalPrice { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime ExpiresAt { get; set; } = DateTime.UtcNow.AddMinutes(10);

    public ICollection<Ticket> Tickets { get; set; } = [];
    public ICollection<SeatLock> SeatLocks { get; set; } = [];
}
