using System.Text.RegularExpressions;
using Grpc.Core;
using Cinema.API.Services;

namespace Cinema.API.Grpc;

public partial class CinemaGrpcService(ICinemaService service) : CinemaGrpc.CinemaGrpcBase
{
    public override async Task<SeatReply> GetSeat(GetSeatRequest request, ServerCallContext context)
    {
        if (!Guid.TryParse(request.Id, out var id))
        {
            throw new RpcException(new Status(StatusCode.InvalidArgument, $"'{request.Id}' is not a valid seat id"));
        }

        var seat = await service.GetSeatByIdAsync(id);
        if (seat == null)
        {
            throw new RpcException(new Status(StatusCode.NotFound, $"Seat {id} not found"));
        }

        return ToSeatReply(seat);
    }

    public override async Task<SeatListReply> GetSeatsByHall(GetSeatsByHallRequest request, ServerCallContext context)
    {
        if (!Guid.TryParse(request.CinemaId, out var cinemaId))
        {
            throw new RpcException(new Status(StatusCode.InvalidArgument, $"'{request.CinemaId}' is not a valid cinema id"));
        }
        if (!Guid.TryParse(request.HallId, out var hallId))
        {
            throw new RpcException(new Status(StatusCode.InvalidArgument, $"'{request.HallId}' is not a valid hall id"));
        }

        var seats = await service.GetSeatsAsync(cinemaId, hallId);

        var reply = new SeatListReply();
        reply.Seats.AddRange(seats.Select(ToSeatReply));
        return reply;
    }

    public override async Task<CinemaReply> GetCinema(GetCinemaRequest request, ServerCallContext context)
    {
        if (!Guid.TryParse(request.Id, out var id))
        {
            throw new RpcException(new Status(StatusCode.InvalidArgument, $"'{request.Id}' is not a valid cinema id"));
        }

        var cinema = await service.GetCinemaByIdAsync(id);
        if (cinema == null)
        {
            throw new RpcException(new Status(StatusCode.NotFound, $"Cinema {id} not found"));
        }

        return new CinemaReply
        {
            Id = cinema.Id.ToString(),
            Name = cinema.Name,
            City = FormatCityName(cinema.City.ToString())
        };
    }

    private static SeatReply ToSeatReply(DTOs.SeatResponse seat) => new()
    {
        Id = seat.Id.ToString(),
        Row = seat.Row,
        Number = seat.Number,
        SeatType = seat.SeatType
    };

    // Matches Converters.CityEnumConverter's JSON formatting ("NoviSad" -> "Novi Sad")
    // so gRPC responses stay consistent with the REST API's output.
    private static string FormatCityName(string enumName) => CityNameRegex().Replace(enumName, "$1 $2");

    [GeneratedRegex("([a-z])([A-Z])")]
    private static partial Regex CityNameRegex();
}
