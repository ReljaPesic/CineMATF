namespace Screening.API.Tests.Unit;

public class ScreeningServiceTests
{
    private readonly Mock<IScreeningRepository> _repositoryMock;
    private readonly IMapper _mapper;
    private readonly ScreeningService _service;

    public ScreeningServiceTests()
    {
        _repositoryMock = new Mock<IScreeningRepository>();

        var config = new MapperConfiguration(cfg => cfg.AddProfile<Mapping.ScreeningMappingProfile>());
        _mapper = config.CreateMapper();

        _service = new ScreeningService(_repositoryMock.Object, _mapper);
    }

    private static Entities.Screening CreateScreening(Guid? id = null, Guid? movieId = null, Guid? hallId = null, Guid? cinemaId = null) => new()
    {
        Id = id ?? Guid.NewGuid(),
        MovieId = movieId ?? Guid.NewGuid(),
        HallId = hallId ?? Guid.NewGuid(),
        CinemaId = cinemaId ?? Guid.NewGuid(),
        StartTime = new DateTime(2026, 9, 1, 18, 0, 0, DateTimeKind.Utc),
        Format = ScreeningFormat.TwoD
    };

    private static ScreeningRequest CreateRequest(Guid? movieId = null, Guid? hallId = null, Guid? cinemaId = null) => new(
        movieId ?? Guid.NewGuid(),
        hallId ?? Guid.NewGuid(),
        cinemaId ?? Guid.NewGuid(),
        new DateTime(2026, 9, 1, 18, 0, 0, DateTimeKind.Utc),
        ScreeningFormat.TwoD
    );

    [Fact]
    public async Task GetScreeningsAsync_ReturnsMappedResponses()
    {
        var screenings = new List<Entities.Screening> { CreateScreening(), CreateScreening() };
        _repositoryMock.Setup(r => r.GetScreeningsAsync(null, null, null)).ReturnsAsync(screenings);

        var result = await _service.GetScreeningsAsync(null, null, null);

        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetScreeningsAsync_WithEmptyResult_ReturnsEmpty()
    {
        _repositoryMock.Setup(r => r.GetScreeningsAsync(null, null, null)).ReturnsAsync([]);

        var result = await _service.GetScreeningsAsync(null, null, null);

        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetScreeningsAsync_PassesFiltersThrough()
    {
        var movieId = Guid.NewGuid();
        var cinemaId = Guid.NewGuid();
        var date = new DateOnly(2026, 9, 1);
        _repositoryMock.Setup(r => r.GetScreeningsAsync(movieId, date, cinemaId)).ReturnsAsync([CreateScreening(movieId: movieId, cinemaId: cinemaId)]);

        var result = await _service.GetScreeningsAsync(movieId, date, cinemaId);

        result.Should().ContainSingle();
        _repositoryMock.Verify(r => r.GetScreeningsAsync(movieId, date, cinemaId), Times.Once);
    }

    [Fact]
    public async Task GetScreeningByIdAsync_WithExistingId_ReturnsResponse()
    {
        var id = Guid.NewGuid();
        _repositoryMock.Setup(r => r.GetScreeningByIdAsync(id)).ReturnsAsync(CreateScreening(id));

        var result = await _service.GetScreeningByIdAsync(id);

        result.Should().NotBeNull();
        result!.Id.Should().Be(id);
    }

    [Fact]
    public async Task GetScreeningByIdAsync_WithNonExistingId_ReturnsNull()
    {
        var id = Guid.NewGuid();
        _repositoryMock.Setup(r => r.GetScreeningByIdAsync(id)).ReturnsAsync((Entities.Screening?)null);

        var result = await _service.GetScreeningByIdAsync(id);

        result.Should().BeNull();
    }

    [Fact]
    public async Task CreateScreeningAsync_MapsRequestAndReturnsCreatedResponse()
    {
        var movieId = Guid.NewGuid();
        var request = CreateRequest(movieId: movieId);
        _repositoryMock.Setup(r => r.CreateScreeningAsync(It.Is<Entities.Screening>(s => s.MovieId == movieId)))
            .ReturnsAsync((Entities.Screening s) => { s.Id = Guid.NewGuid(); return s; });

        var result = await _service.CreateScreeningAsync(request);

        result.Should().NotBeNull();
        result.MovieId.Should().Be(movieId);
        result.Id.Should().NotBe(Guid.Empty);
    }

    [Fact]
    public async Task UpdateScreeningAsync_WithExistingId_ReturnsUpdatedResponse()
    {
        var id = Guid.NewGuid();
        var existing = CreateScreening(id);
        var newMovieId = Guid.NewGuid();
        var request = CreateRequest(movieId: newMovieId);

        _repositoryMock.Setup(r => r.GetScreeningByIdAsync(id)).ReturnsAsync(existing);
        _repositoryMock.Setup(r => r.UpdateScreeningAsync(It.Is<Entities.Screening>(s => s.Id == id))).ReturnsAsync(true);

        var result = await _service.UpdateScreeningAsync(id, request);

        result.Should().NotBeNull();
        result!.Id.Should().Be(id);
        result.MovieId.Should().Be(newMovieId);
    }

    [Fact]
    public async Task UpdateScreeningAsync_WithNonExistingId_ReturnsNullWithoutCallingRepositoryUpdate()
    {
        var id = Guid.NewGuid();
        _repositoryMock.Setup(r => r.GetScreeningByIdAsync(id)).ReturnsAsync((Entities.Screening?)null);

        var result = await _service.UpdateScreeningAsync(id, CreateRequest());

        result.Should().BeNull();
        _repositoryMock.Verify(r => r.UpdateScreeningAsync(It.IsAny<Entities.Screening>()), Times.Never);
    }

    [Fact]
    public async Task UpdateScreeningAsync_WhenRepositoryUpdateFails_ReturnsNull()
    {
        var id = Guid.NewGuid();
        _repositoryMock.Setup(r => r.GetScreeningByIdAsync(id)).ReturnsAsync(CreateScreening(id));
        _repositoryMock.Setup(r => r.UpdateScreeningAsync(It.IsAny<Entities.Screening>())).ReturnsAsync(false);

        var result = await _service.UpdateScreeningAsync(id, CreateRequest());

        result.Should().BeNull();
    }

    [Fact]
    public async Task DeleteScreeningAsync_WithExistingId_ReturnsTrue()
    {
        var id = Guid.NewGuid();
        _repositoryMock.Setup(r => r.DeleteScreeningAsync(id)).ReturnsAsync(true);

        var result = await _service.DeleteScreeningAsync(id);

        result.Should().BeTrue();
    }

    [Fact]
    public async Task DeleteScreeningAsync_WithNonExistingId_ReturnsFalse()
    {
        var id = Guid.NewGuid();
        _repositoryMock.Setup(r => r.DeleteScreeningAsync(id)).ReturnsAsync(false);

        var result = await _service.DeleteScreeningAsync(id);

        result.Should().BeFalse();
    }
}
