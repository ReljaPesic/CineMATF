namespace Reservation.API.Domain.Entities;

public class SeatLock
{
    public Guid Id { get; set; }
    public Guid ScreeningId { get; set; }
    public Guid SeatId { get; set; }
    public Guid UserId { get; set; }
    public DateTime LockedAt { get; set; } = DateTime.UtcNow;
    public DateTime ExpiresAt { get; set; }
    public Guid? ReservationId { get; set; }

    public Reservation? Reservation { get; set; }
}
