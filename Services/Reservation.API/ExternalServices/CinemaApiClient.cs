using System.Net;
using System.Net.Http.Json;

namespace Reservation.API.ExternalServices;

public class CinemaApiClient(HttpClient httpClient) : ICinemaApiClient
{
    private record CinemaSeatResponse(Guid Id, int Row, int Number, string SeatType);

    public async Task<SeatDetails?> GetSeatAsync(Guid seatId, CancellationToken cancellationToken = default)
    {
        var response = await httpClient.GetAsync($"api/v1/cinema/seats/{seatId}", cancellationToken);
        if (response.StatusCode == HttpStatusCode.NotFound) return null;
        response.EnsureSuccessStatusCode();

        var seat = await response.Content.ReadFromJsonAsync<CinemaSeatResponse>(cancellationToken);
        return seat == null ? null : new SeatDetails(seat.Id, seat.Row, seat.Number, seat.SeatType);
    }

    public async Task<IEnumerable<SeatDetails>> GetSeatsByHallAsync(Guid cinemaId, Guid hallId, CancellationToken cancellationToken = default)
    {
        var response = await httpClient.GetAsync($"api/v1/cinema/{cinemaId}/halls/{hallId}/seats", cancellationToken);
        response.EnsureSuccessStatusCode();

        var seats = await response.Content.ReadFromJsonAsync<IEnumerable<CinemaSeatResponse>>(cancellationToken);
        return seats?.Select(s => new SeatDetails(s.Id, s.Row, s.Number, s.SeatType)) ?? [];
    }
}
