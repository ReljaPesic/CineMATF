using Grpc.Core;
using Screening.API.Grpc;

namespace Reservation.API.ExternalServices;

public class ScreeningApiClient(ScreeningGrpc.ScreeningGrpcClient client) : IScreeningApiClient
{
    public async Task<ScreeningDetails?> GetScreeningAsync(Guid screeningId, CancellationToken cancellationToken = default)
    {
        try
        {
            var reply = await client.GetScreeningAsync(
                new GetScreeningRequest { Id = screeningId.ToString() },
                cancellationToken: cancellationToken);

            return new ScreeningDetails(
                Guid.Parse(reply.Id),
                Guid.Parse(reply.MovieId),
                Guid.Parse(reply.HallId),
                Guid.Parse(reply.CinemaId),
                reply.StartTime.ToDateTime(),
                reply.Format,
                reply.Status);
        }
        catch (RpcException ex) when (ex.StatusCode == StatusCode.NotFound)
        {
            return null;
        }
    }
}
