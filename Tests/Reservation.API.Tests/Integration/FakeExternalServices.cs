using Reservation.API.ExternalServices;

namespace Reservation.API.Tests.Integration;

// Reservation.API validates screenings/seats against Cinema.API, Movie.API and
// Screening.API over the network. These fakes stand in for those services so the
// ownership/authorization tests can exercise the real ReservationService/Repository
// against an in-memory database without any other service running.
internal class FakeScreeningApiClient : IScreeningApiClient
{
    public Task<ScreeningDetails?> GetScreeningAsync(Guid screeningId, CancellationToken cancellationToken = default) =>
        Task.FromResult<ScreeningDetails?>(new ScreeningDetails(
            screeningId, Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), DateTime.UtcNow.AddDays(1), "2D"));
}

internal class FakeCinemaApiClient : ICinemaApiClient
{
    public Task<SeatDetails?> GetSeatAsync(Guid seatId, CancellationToken cancellationToken = default) =>
        Task.FromResult<SeatDetails?>(new SeatDetails(seatId, 1, 1, "Standard"));

    public Task<IEnumerable<SeatDetails>> GetSeatsByHallAsync(Guid cinemaId, Guid hallId, CancellationToken cancellationToken = default) =>
        Task.FromResult<IEnumerable<SeatDetails>>([]);

    public Task<CinemaDetails?> GetCinemaAsync(Guid cinemaId, CancellationToken cancellationToken = default) =>
        Task.FromResult<CinemaDetails?>(new CinemaDetails(cinemaId, "CineMax", "Beograd"));
}

internal class FakeMovieApiClient : IMovieApiClient
{
    public Task<MovieDetails?> GetMovieAsync(Guid movieId, CancellationToken cancellationToken = default) =>
        Task.FromResult<MovieDetails?>(new MovieDetails(movieId, "Test Movie"));
}
