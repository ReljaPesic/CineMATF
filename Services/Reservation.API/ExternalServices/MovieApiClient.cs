using Movie.API.Grpc;
using Grpc.Core;

namespace Reservation.API.ExternalServices;

public class MovieApiClient(MovieGrpc.MovieGrpcClient client) : IMovieApiClient
{
    public async Task<MovieDetails?> GetMovieAsync(Guid movieId, CancellationToken cancellationToken = default)
    {
        try
        {
            var reply = await client.GetMovieAsync(
                new GetMovieRequest { Id = movieId.ToString() },
                cancellationToken: cancellationToken);

            return new MovieDetails(Guid.Parse(reply.Id), reply.Title);
        }
        catch (RpcException ex) when (ex.StatusCode == StatusCode.NotFound)
        {
            return null;
        }
    }
}
