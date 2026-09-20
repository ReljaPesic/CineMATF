using Grpc.Core;
using Movie.API.Services;

namespace Movie.API.Grpc;

public class MovieGrpcService(IMovieService service) : MovieGrpc.MovieGrpcBase
{
    public override async Task<MovieReply> GetMovie(GetMovieRequest request, ServerCallContext context)
    {
        if (!Guid.TryParse(request.Id, out var id))
        {
            throw new RpcException(new Status(StatusCode.InvalidArgument, $"'{request.Id}' is not a valid movie id"));
        }

        var movie = await service.GetMovieByIdAsync(id);
        if (movie == null)
        {
            throw new RpcException(new Status(StatusCode.NotFound, $"Movie {id} not found"));
        }

        return new MovieReply
        {
            Id = movie.Id.ToString(),
            Title = movie.Title
        };
    }
}
