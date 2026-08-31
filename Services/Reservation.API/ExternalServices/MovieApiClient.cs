using System.Net;
using System.Net.Http.Json;

namespace Reservation.API.ExternalServices;

public class MovieApiClient(HttpClient httpClient) : IMovieApiClient
{
    private record MovieApiResponse(Guid Id, string Title);

    public async Task<MovieDetails?> GetMovieAsync(Guid movieId, CancellationToken cancellationToken = default)
    {
        var response = await httpClient.GetAsync($"api/v1/movie/{movieId}", cancellationToken);
        if (response.StatusCode == HttpStatusCode.NotFound) return null;
        response.EnsureSuccessStatusCode();

        var movie = await response.Content.ReadFromJsonAsync<MovieApiResponse>(cancellationToken);
        return movie == null ? null : new MovieDetails(movie.Id, movie.Title);
    }
}
