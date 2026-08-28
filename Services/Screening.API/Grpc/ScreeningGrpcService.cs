using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Screening.API.Services;

namespace Screening.API.Grpc;

public class ScreeningGrpcService(IScreeningService service) : ScreeningGrpc.ScreeningGrpcBase
{
    private readonly IScreeningService _service = service ?? throw new ArgumentNullException(nameof(service));

    public override async Task<GetScreeningReply> GetScreening(GetScreeningRequest request, ServerCallContext context)
    {
        if (!Guid.TryParse(request.Id, out var id))
        {
            throw new RpcException(new Status(StatusCode.InvalidArgument, $"'{request.Id}' is not a valid screening id"));
        }

        var screening = await _service.GetScreeningByIdAsync(id);
        if (screening == null)
        {
            throw new RpcException(new Status(StatusCode.NotFound, $"Screening {id} not found"));
        }

        return new GetScreeningReply
        {
            Id = screening.Id.ToString(),
            MovieId = screening.MovieId.ToString(),
            HallId = screening.HallId.ToString(),
            CinemaId = screening.CinemaId.ToString(),
            StartTime = Timestamp.FromDateTime(DateTime.SpecifyKind(screening.StartTime, DateTimeKind.Utc)),
            Format = screening.Format.ToString(),
            Status = screening.Status.ToString()
        };
    }
}
