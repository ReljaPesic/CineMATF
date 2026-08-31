using Google.Protobuf.WellKnownTypes;

namespace Screening.API.Tests.Unit;

public class ScreeningGrpcServiceTests
{
    private readonly Mock<IScreeningService> _serviceMock;
    private readonly ScreeningGrpcService _grpcService;

    public ScreeningGrpcServiceTests()
    {
        _serviceMock = new Mock<IScreeningService>();
        _grpcService = new ScreeningGrpcService(_serviceMock.Object);
    }

    private static ScreeningResponse CreateResponse(Guid id) => new(
        id,
        Guid.NewGuid(),
        Guid.NewGuid(),
        Guid.NewGuid(),
        new DateTime(2026, 9, 1, 18, 0, 0, DateTimeKind.Utc),
        ScreeningFormat.IMAX
    );

    [Fact]
    public async Task GetScreening_WithInvalidGuid_ThrowsInvalidArgument()
    {
        var request = new GetScreeningRequest { Id = "not-a-guid" };

        var action = () => _grpcService.GetScreening(request, null!);

        var exception = await action.Should().ThrowAsync<RpcException>();
        exception.Which.StatusCode.Should().Be(StatusCode.InvalidArgument);
        _serviceMock.Verify(s => s.GetScreeningByIdAsync(It.IsAny<Guid>()), Times.Never);
    }

    [Fact]
    public async Task GetScreening_WithNonExistentScreening_ThrowsNotFound()
    {
        var id = Guid.NewGuid();
        _serviceMock.Setup(s => s.GetScreeningByIdAsync(id)).ReturnsAsync((ScreeningResponse?)null);
        var request = new GetScreeningRequest { Id = id.ToString() };

        var action = () => _grpcService.GetScreening(request, null!);

        var exception = await action.Should().ThrowAsync<RpcException>();
        exception.Which.StatusCode.Should().Be(StatusCode.NotFound);
    }

    [Fact]
    public async Task GetScreening_WithExistingScreening_ReturnsMappedReply()
    {
        var id = Guid.NewGuid();
        var response = CreateResponse(id);
        _serviceMock.Setup(s => s.GetScreeningByIdAsync(id)).ReturnsAsync(response);
        var request = new GetScreeningRequest { Id = id.ToString() };

        var reply = await _grpcService.GetScreening(request, null!);

        reply.Id.Should().Be(response.Id.ToString());
        reply.MovieId.Should().Be(response.MovieId.ToString());
        reply.HallId.Should().Be(response.HallId.ToString());
        reply.CinemaId.Should().Be(response.CinemaId.ToString());
        reply.Format.Should().Be("IMAX");
        reply.StartTime.Should().Be(Timestamp.FromDateTime(response.StartTime));
    }
}
