namespace Reservation.API.ExternalServices;

public interface ICinemaApiClient
{
    Task<SeatDetails?> GetSeatAsync(Guid seatId, CancellationToken cancellationToken = default);
    Task<IEnumerable<SeatDetails>> GetSeatsByHallAsync(Guid cinemaId, Guid hallId, CancellationToken cancellationToken = default);
    Task<CinemaDetails?> GetCinemaAsync(Guid cinemaId, CancellationToken cancellationToken = default);
}
