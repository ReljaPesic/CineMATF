using Cinema.API.Grpc;
using Grpc.Core;

namespace Reservation.API.ExternalServices;

public class CinemaApiClient(CinemaGrpc.CinemaGrpcClient client) : ICinemaApiClient
{
    public async Task<SeatDetails?> GetSeatAsync(Guid seatId, CancellationToken cancellationToken = default)
    {
        try
        {
            var reply = await client.GetSeatAsync(
                new GetSeatRequest { Id = seatId.ToString() },
                cancellationToken: cancellationToken);

            return new SeatDetails(Guid.Parse(reply.Id), reply.Row, reply.Number, reply.SeatType);
        }
        catch (RpcException ex) when (ex.StatusCode == StatusCode.NotFound)
        {
            return null;
        }
    }

    public async Task<IEnumerable<SeatDetails>> GetSeatsByHallAsync(Guid cinemaId, Guid hallId, CancellationToken cancellationToken = default)
    {
        var reply = await client.GetSeatsByHallAsync(
            new GetSeatsByHallRequest { CinemaId = cinemaId.ToString(), HallId = hallId.ToString() },
            cancellationToken: cancellationToken);

        return reply.Seats.Select(s => new SeatDetails(Guid.Parse(s.Id), s.Row, s.Number, s.SeatType));
    }

    public async Task<CinemaDetails?> GetCinemaAsync(Guid cinemaId, CancellationToken cancellationToken = default)
    {
        try
        {
            var reply = await client.GetCinemaAsync(
                new GetCinemaRequest { Id = cinemaId.ToString() },
                cancellationToken: cancellationToken);

            return new CinemaDetails(Guid.Parse(reply.Id), reply.Name, reply.City);
        }
        catch (RpcException ex) when (ex.StatusCode == StatusCode.NotFound)
        {
            return null;
        }
    }
}
