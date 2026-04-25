namespace Cinema.API.Tests.Unit;

public class CinemaServiceTests
{
    private readonly Mock<ICinemaRepository> _repositoryMock;
    private readonly IMapper _mapper;
    private readonly CinemaService _service;

    public CinemaServiceTests()
    {
        _repositoryMock = new Mock<ICinemaRepository>();

        var config = new MapperConfiguration(cfg =>
        {
            cfg.AddProfile<Mapping.CinemaMappingProfile>();
            cfg.AddProfile<Mapping.HallMappingProfile>();
        });
        _mapper = config.CreateMapper();

        _service = new CinemaService(_repositoryMock.Object, _mapper);
    }

    [Fact]
    public async Task CreateCinemaAsync_WithValidRequest_ReturnsCinemaResponse()
    {
        var request = new CinemaRequest("CineMax", City.Beograd);
        var cinema = new MovieTheatre { Id = Guid.NewGuid(), Name = "CineMax", City = City.Beograd };
        _repositoryMock.Setup(r => r.CreateCinemaAsync(request)).ReturnsAsync(cinema);

        var result = await _service.CreateCinemaAsync(request);

        result.Should().NotBeNull();
        result.Name.Should().Be("CineMax");
        result.City.Should().Be(City.Beograd);
    }

    [Fact]
    public async Task GetCinemasAsync_ReturnsPagedResponse()
    {
        var cinemas = new List<MovieTheatre>
        {
            new() { Id = Guid.NewGuid(), Name = "CineMax", City = City.Beograd },
            new() { Id = Guid.NewGuid(), Name = "Cineplexx", City = City.NoviSad }
        };
        _repositoryMock.Setup(r => r.GetCinemasAsync(1, 10)).ReturnsAsync((cinemas, 2));

        var result = await _service.GetCinemasAsync(1, 10);

        result.Should().NotBeNull();
        result.Data.Should().HaveCount(2);
        result.TotalCount.Should().Be(2);
    }

    [Fact]
    public async Task GetCinemasByCityAsync_WithValidCity_ReturnsCinemas()
    {
        var cinemas = new List<MovieTheatre>
        {
            new() { Id = Guid.NewGuid(), Name = "CineMax", City = City.Beograd }
        };
        _repositoryMock.Setup(r => r.GetCinemasByCityAsync(City.Beograd)).ReturnsAsync(cinemas);

        var result = await _service.GetCinemasByCityAsync(City.Beograd);

        result.Should().HaveCount(1);
        result.First().City.Should().Be(City.Beograd);
    }

    [Fact]
    public async Task GetCinemaByIdAsync_WithExistingId_ReturnsCinemaResponse()
    {
        var id = Guid.NewGuid();
        var cinema = new MovieTheatre { Id = id, Name = "CineMax", City = City.Beograd };
        _repositoryMock.Setup(r => r.GetCinemaByIdAsync(id)).ReturnsAsync(cinema);

        var result = await _service.GetCinemaByIdAsync(id);

        result.Should().NotBeNull();
        result!.Id.Should().Be(id);
    }

    [Fact]
    public async Task GetCinemaByIdAsync_WithNonExistingId_ReturnsNull()
    {
        var id = Guid.NewGuid();
        _repositoryMock.Setup(r => r.GetCinemaByIdAsync(id)).ReturnsAsync((MovieTheatre?)null);

        var result = await _service.GetCinemaByIdAsync(id);

        result.Should().BeNull();
    }

    [Fact]
    public async Task DeleteCinemaAsync_WithExistingId_ReturnsTrue()
    {
        var id = Guid.NewGuid();
        _repositoryMock.Setup(r => r.DeleteCinemaAsync(id)).ReturnsAsync(true);

        var result = await _service.DeleteCinemaAsync(id);

        result.Should().BeTrue();
    }

    [Fact]
    public async Task DeleteCinemaAsync_WithNonExistingId_ReturnsFalse()
    {
        var id = Guid.NewGuid();
        _repositoryMock.Setup(r => r.DeleteCinemaAsync(id)).ReturnsAsync(false);

        var result = await _service.DeleteCinemaAsync(id);

        result.Should().BeFalse();
    }

    [Fact]
    public async Task UpdateCinemaAsync_WithExistingId_ReturnsUpdatedCinema()
    {
        var id = Guid.NewGuid();
        var existing = new MovieTheatre { Id = id, Name = "OldName", City = City.Beograd };
        var request = new CinemaRequest("NewName", City.NoviSad);
        _repositoryMock.Setup(r => r.GetCinemaByIdAsync(id)).ReturnsAsync(existing);

        var result = await _service.UpdateCinemaAsync(id, request);

        result.Should().NotBeNull();
        result!.Name.Should().Be("NewName");
        result.City.Should().Be(City.NoviSad);
    }

    [Fact]
    public async Task UpdateCinemaAsync_WithNonExistingId_ReturnsNull()
    {
        var id = Guid.NewGuid();
        var request = new CinemaRequest("NewName", City.NoviSad);
        _repositoryMock.Setup(r => r.GetCinemaByIdAsync(id)).ReturnsAsync((MovieTheatre?)null);

        var result = await _service.UpdateCinemaAsync(id, request);

        result.Should().BeNull();
    }

    [Fact]
    public async Task CreateHallsAsync_WithValidRequests_ReturnsCreatedCount()
    {
        var cinemaId = Guid.NewGuid();
        var requests = new List<HallRequest>
        {
            new("Hall 1", 5, 10),
            new("Hall 2", 3, 8)
        };
        var halls = requests.Select(r => new Hall
        {
            Id = Guid.NewGuid(),
            Name = r.Name,
            TotalRows = r.TotalRows,
            SeatsPerRow = r.SeatsPerRow,
            CinemaId = cinemaId
        }).ToList();

        _repositoryMock.SetupSequence(r => r.CreateHallAsync(cinemaId, It.IsAny<HallRequest>()))
            .ReturnsAsync(halls[0])
            .ReturnsAsync(halls[1]);

        var result = await _service.CreateHallsAsync(cinemaId, requests);

        result.Created.Should().Be(2);
    }

    [Fact]
    public async Task GetHallsAsync_WithValidCinemaId_ReturnsHalls()
    {
        var cinemaId = Guid.NewGuid();
        var halls = new List<Hall>
        {
            new() { Id = Guid.NewGuid(), Name = "Hall 1", CinemaId = cinemaId },
            new() { Id = Guid.NewGuid(), Name = "Hall 2", CinemaId = cinemaId }
        };
        _repositoryMock.Setup(r => r.GetHallsAsync(cinemaId)).ReturnsAsync(halls);

        var result = await _service.GetHallsAsync(cinemaId);

        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task DeleteHallAsync_WithExistingHall_ReturnsTrue()
    {
        var cinemaId = Guid.NewGuid();
        var hallId = Guid.NewGuid();
        _repositoryMock.Setup(r => r.DeleteHallAsync(cinemaId, hallId)).ReturnsAsync(true);

        var result = await _service.DeleteHallAsync(cinemaId, hallId);

        result.Should().BeTrue();
    }
}
