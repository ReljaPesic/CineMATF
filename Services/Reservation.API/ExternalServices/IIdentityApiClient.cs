namespace Reservation.API.ExternalServices;

public interface IIdentityApiClient
{
    Task<UserContactDetails?> GetUserAsync(Guid userId, CancellationToken cancellationToken = default);
}
