namespace Reservation.API.ExternalServices;

public interface ICinemaApiClient
{
    Task<SeatDetails?> GetSeatAsync(Guid seatId, CancellationToken cancellationToken = default);
}
