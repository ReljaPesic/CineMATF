namespace Screening.API.Tests.Unit;

public class ScreeningServiceTests
{
    private readonly Mock<IScreeningRepository> _repositoryMock;
    private readonly Mock<IMovieApiClient> _movieApiClientMock;
    private readonly IMapper _mapper;
    private readonly ScreeningService _service;

    public ScreeningServiceTests()
    {
        _repositoryMock = new Mock<IScreeningRepository>();
        // No other screenings in the hall unless a test says otherwise - keeps the
        // occupancy check out of the way of tests that aren't about it.
        _repositoryMock.Setup(r => r.GetScreeningsByHallAsync(It.IsAny<Guid>())).ReturnsAsync([]);

        _movieApiClientMock = new Mock<IMovieApiClient>();
        _movieApiClientMock.Setup(c => c.GetMovieAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Guid id, CancellationToken _) => new MovieDetails(id, "Test Movie", 120));

        var config = new MapperConfiguration(cfg => cfg.AddProfile<Mapping.ScreeningMappingProfile>());
        _mapper = config.CreateMapper();

        _service = new ScreeningService(_repositoryMock.Object, _mapper, _movieApiClientMock.Object);
    }

    private static Entities.Screening CreateScreening(Guid? id = null, Guid? movieId = null, Guid? hallId = null, Guid? cinemaId = null, DateTime? startTime = null) => new()
    {
        Id = id ?? Guid.NewGuid(),
        MovieId = movieId ?? Guid.NewGuid(),
        HallId = hallId ?? Guid.NewGuid(),
        CinemaId = cinemaId ?? Guid.NewGuid(),
        StartTime = startTime ?? new DateTime(2026, 9, 1, 18, 0, 0, DateTimeKind.Utc),
        Format = ScreeningFormat.TwoD
    };

    private static ScreeningRequest CreateRequest(Guid? movieId = null, Guid? hallId = null, Guid? cinemaId = null, DateTime? startTime = null) => new(
        movieId ?? Guid.NewGuid(),
        hallId ?? Guid.NewGuid(),
        cinemaId ?? Guid.NewGuid(),
        startTime ?? new DateTime(2026, 9, 1, 18, 0, 0, DateTimeKind.Utc),
        ScreeningFormat.TwoD
    );

    [Fact]
    public async Task GetScreeningsAsync_ReturnsMappedResponses()
    {
        var screenings = new List<Entities.Screening> { CreateScreening(), CreateScreening() };
        _repositoryMock.Setup(r => r.GetScreeningsAsync(null, null, null, null)).ReturnsAsync(screenings);

        var result = await _service.GetScreeningsAsync(null, null, null, null);

        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetScreeningsAsync_WithEmptyResult_ReturnsEmpty()
    {
        _repositoryMock.Setup(r => r.GetScreeningsAsync(null, null, null, null)).ReturnsAsync([]);

        var result = await _service.GetScreeningsAsync(null, null, null, null);

        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetScreeningsAsync_PassesFiltersThrough()
    {
        var movieId = Guid.NewGuid();
        var cinemaId = Guid.NewGuid();
        var date = new DateOnly(2026, 9, 1);
        _repositoryMock.Setup(r => r.GetScreeningsAsync(movieId, date, cinemaId, null)).ReturnsAsync([CreateScreening(movieId: movieId, cinemaId: cinemaId)]);

        var result = await _service.GetScreeningsAsync(movieId, date, cinemaId, null);

        result.Should().ContainSingle();
        _repositoryMock.Verify(r => r.GetScreeningsAsync(movieId, date, cinemaId, null), Times.Once);
    }

    [Fact]
    public async Task GetScreeningsAsync_PassesRestrictToCinemaIdsThrough()
    {
        var restrictToCinemaIds = new[] { Guid.NewGuid(), Guid.NewGuid() };
        _repositoryMock.Setup(r => r.GetScreeningsAsync(null, null, null, restrictToCinemaIds)).ReturnsAsync([CreateScreening()]);

        var result = await _service.GetScreeningsAsync(null, null, null, restrictToCinemaIds);

        result.Should().ContainSingle();
        _repositoryMock.Verify(r => r.GetScreeningsAsync(null, null, null, restrictToCinemaIds), Times.Once);
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

        result.Success.Should().BeTrue();
        result.Response.Should().NotBeNull();
        result.Response!.MovieId.Should().Be(movieId);
        result.Response.Id.Should().NotBe(Guid.Empty);
    }

    [Fact]
    public async Task CreateScreeningAsync_WhenMovieNotFound_ReturnsFailureWithoutCallingRepositoryCreate()
    {
        var movieId = Guid.NewGuid();
        _movieApiClientMock.Setup(c => c.GetMovieAsync(movieId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((MovieDetails?)null);

        var result = await _service.CreateScreeningAsync(CreateRequest(movieId: movieId));

        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Be("Movie not found");
        _repositoryMock.Verify(r => r.CreateScreeningAsync(It.IsAny<Entities.Screening>()), Times.Never);
    }

    [Fact]
    public async Task CreateScreeningAsync_WhenHallOccupiedByAnotherScreening_ReturnsFailure()
    {
        var hallId = Guid.NewGuid();
        var existingStart = new DateTime(2026, 9, 1, 18, 0, 0, DateTimeKind.Utc);
        // 120 min runtime + 30 min buffer -> occupied 18:00-20:30. A 20:00 start overlaps.
        _repositoryMock.Setup(r => r.GetScreeningsByHallAsync(hallId))
            .ReturnsAsync([CreateScreening(hallId: hallId, startTime: existingStart)]);

        var request = CreateRequest(hallId: hallId, startTime: existingStart.AddHours(2));

        var result = await _service.CreateScreeningAsync(request);

        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Contain("occupied");
        _repositoryMock.Verify(r => r.CreateScreeningAsync(It.IsAny<Entities.Screening>()), Times.Never);
    }

    [Fact]
    public async Task CreateScreeningAsync_WhenHallFreeAfterBuffer_Succeeds()
    {
        var hallId = Guid.NewGuid();
        var existingStart = new DateTime(2026, 9, 1, 18, 0, 0, DateTimeKind.Utc);
        // 120 min runtime + 30 min buffer -> occupied until 20:30. A 20:30 start doesn't overlap.
        _repositoryMock.Setup(r => r.GetScreeningsByHallAsync(hallId))
            .ReturnsAsync([CreateScreening(hallId: hallId, startTime: existingStart)]);
        _repositoryMock.Setup(r => r.CreateScreeningAsync(It.IsAny<Entities.Screening>()))
            .ReturnsAsync((Entities.Screening s) => { s.Id = Guid.NewGuid(); return s; });

        var request = CreateRequest(hallId: hallId, startTime: existingStart.AddMinutes(150));

        var result = await _service.CreateScreeningAsync(request);

        result.Success.Should().BeTrue();
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

        result.Success.Should().BeTrue();
        result.Response.Should().NotBeNull();
        result.Response!.Id.Should().Be(id);
        result.Response.MovieId.Should().Be(newMovieId);
    }

    [Fact]
    public async Task UpdateScreeningAsync_ExcludesItsOwnScreeningFromTheConflictCheck()
    {
        var id = Guid.NewGuid();
        var hallId = Guid.NewGuid();
        var startTime = new DateTime(2026, 9, 1, 18, 0, 0, DateTimeKind.Utc);
        var existing = CreateScreening(id, hallId: hallId, startTime: startTime);

        _repositoryMock.Setup(r => r.GetScreeningByIdAsync(id)).ReturnsAsync(existing);
        // The only screening in the hall is the one being updated - re-saving the same
        // time must not conflict with itself.
        _repositoryMock.Setup(r => r.GetScreeningsByHallAsync(hallId)).ReturnsAsync([existing]);
        _repositoryMock.Setup(r => r.UpdateScreeningAsync(It.IsAny<Entities.Screening>())).ReturnsAsync(true);

        var result = await _service.UpdateScreeningAsync(id, CreateRequest(hallId: hallId, startTime: startTime));

        result.Success.Should().BeTrue();
    }

    [Fact]
    public async Task UpdateScreeningAsync_WithNonExistingId_ReturnsFailureWithoutCallingRepositoryUpdate()
    {
        var id = Guid.NewGuid();
        _repositoryMock.Setup(r => r.GetScreeningByIdAsync(id)).ReturnsAsync((Entities.Screening?)null);

        var result = await _service.UpdateScreeningAsync(id, CreateRequest());

        result.Success.Should().BeFalse();
        _repositoryMock.Verify(r => r.UpdateScreeningAsync(It.IsAny<Entities.Screening>()), Times.Never);
    }

    [Fact]
    public async Task UpdateScreeningAsync_WhenRepositoryUpdateFails_ReturnsFailure()
    {
        var id = Guid.NewGuid();
        _repositoryMock.Setup(r => r.GetScreeningByIdAsync(id)).ReturnsAsync(CreateScreening(id));
        _repositoryMock.Setup(r => r.UpdateScreeningAsync(It.IsAny<Entities.Screening>())).ReturnsAsync(false);

        var result = await _service.UpdateScreeningAsync(id, CreateRequest());

        result.Success.Should().BeFalse();
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
