namespace Reservation.API.ExternalServices;

public interface IScreeningApiClient
{
    Task<ScreeningDetails?> GetScreeningAsync(Guid screeningId, CancellationToken cancellationToken = default);
}
