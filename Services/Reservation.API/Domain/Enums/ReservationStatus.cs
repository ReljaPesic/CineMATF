namespace Reservation.API.Domain.Enums;

public enum ReservationStatus
{
    Pending = 0,
    Confirmed = 1,
    Cancelled = 2,
    Expired = 3,
    Locked = 4
}
