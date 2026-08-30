namespace Reservation.API.ExternalServices;

public interface IMovieApiClient
{
    Task<MovieDetails?> GetMovieAsync(Guid movieId, CancellationToken cancellationToken = default);
}
