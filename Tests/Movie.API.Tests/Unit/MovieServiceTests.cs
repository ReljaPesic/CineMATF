namespace Movie.API.Tests.Unit;

public class MovieServiceTests
{
    private readonly Mock<IMovieRepository> _repositoryMock;
    private readonly IMapper _mapper;
    private readonly MovieService _service;

    public MovieServiceTests()
    {
        _repositoryMock = new Mock<IMovieRepository>();

        var config = new MapperConfiguration(cfg => cfg.AddProfile<Mapping.MovieMappingProfile>());
        _mapper = config.CreateMapper();

        _service = new MovieService(_repositoryMock.Object, _mapper);
    }

    private static Entities.Movie CreateMovie(Guid? id = null, string title = "Inception") => new()
    {
        Id = id ?? Guid.NewGuid(),
        Title = title,
        Description = "A mind-bending thriller.",
        DurationMinutes = 148,
        ReleaseDate = new DateTime(2010, 7, 16),
        Rating = 8.8,
        Genres = [Genre.SciFi],
        Actors = [new Actor { FirstName = "Leonardo", LastName = "DiCaprio" }]
    };

    private static MovieRequest CreateRequest(string title = "Inception") => new(
        title,
        "A mind-bending thriller.",
        148,
        new DateTime(2010, 7, 16),
        8.8,
        [new Actor { FirstName = "Leonardo", LastName = "DiCaprio" }],
        [Genre.SciFi],
        null
    );

    [Fact]
    public async Task CreateMovieAsync_WithValidRequest_ReturnsMovieResponse()
    {
        var request = CreateRequest();
        var created = CreateMovie();
        _repositoryMock.Setup(r => r.CreateMovieAsync(It.IsAny<Entities.Movie>())).ReturnsAsync(created);

        var result = await _service.CreateMovieAsync(request);

        result.Should().NotBeNull();
        result.Title.Should().Be("Inception");
    }

    [Fact]
    public async Task GetMoviesAsync_ReturnsPagedResponse()
    {
        var movies = new List<Entities.Movie> { CreateMovie(), CreateMovie(title: "The Godfather") };
        _repositoryMock.Setup(r => r.GetMoviesAsync(1, 10)).ReturnsAsync((movies, 2));

        var result = await _service.GetMoviesAsync(1, 10);

        result.Data.Should().HaveCount(2);
        result.TotalCount.Should().Be(2);
    }

    [Fact]
    public async Task GetMoviesAsync_WithEmptyList_ReturnsEmptyData()
    {
        _repositoryMock.Setup(r => r.GetMoviesAsync(1, 10)).ReturnsAsync((new List<Entities.Movie>(), 0));

        var result = await _service.GetMoviesAsync(1, 10);

        result.Data.Should().BeEmpty();
        result.TotalCount.Should().Be(0);
    }

    [Fact]
    public async Task GetMovieByIdAsync_WithExistingId_ReturnsMovieResponse()
    {
        var id = Guid.NewGuid();
        _repositoryMock.Setup(r => r.GetMovieByIdAsync(id)).ReturnsAsync(CreateMovie(id));

        var result = await _service.GetMovieByIdAsync(id);

        result.Should().NotBeNull();
        result!.Id.Should().Be(id);
    }

    [Fact]
    public async Task GetMovieByIdAsync_WithNonExistingId_ReturnsNull()
    {
        var id = Guid.NewGuid();
        _repositoryMock.Setup(r => r.GetMovieByIdAsync(id)).ReturnsAsync((Entities.Movie?)null);

        var result = await _service.GetMovieByIdAsync(id);

        result.Should().BeNull();
    }

    [Fact]
    public async Task GetMoviesByGenreAsync_ReturnsMatchingMovies()
    {
        var movies = new List<Entities.Movie> { CreateMovie() };
        _repositoryMock.Setup(r => r.GetMoviesByGenreAsync(Genre.SciFi)).ReturnsAsync(movies);

        var result = await _service.GetMoviesByGenreAsync(Genre.SciFi);

        result.Should().HaveCount(1);
    }

    [Fact]
    public async Task GetMoviesByTitleAsync_ReturnsMatchingMovies()
    {
        var movies = new List<Entities.Movie> { CreateMovie() };
        _repositoryMock.Setup(r => r.GetMoviesByTitleAsync("Incep")).ReturnsAsync(movies);

        var result = await _service.GetMoviesByTitleAsync("Incep");

        result.Should().HaveCount(1);
    }

    [Fact]
    public async Task UpdateMovieAsync_WithExistingId_ReturnsUpdatedMovie()
    {
        var id = Guid.NewGuid();
        var existing = CreateMovie(id);
        var request = CreateRequest(title: "Inception 2");

        _repositoryMock.Setup(r => r.GetMovieByIdAsync(id)).ReturnsAsync(existing);
        _repositoryMock.Setup(r => r.UpdateMovieAsync(It.Is<Entities.Movie>(m => m.Id == id))).ReturnsAsync(true);

        var result = await _service.UpdateMovieAsync(id, request);

        result.Should().NotBeNull();
        result!.Title.Should().Be("Inception 2");
    }

    [Fact]
    public async Task UpdateMovieAsync_WithNonExistingId_ReturnsNull()
    {
        var id = Guid.NewGuid();
        _repositoryMock.Setup(r => r.GetMovieByIdAsync(id)).ReturnsAsync((Entities.Movie?)null);

        var result = await _service.UpdateMovieAsync(id, CreateRequest());

        result.Should().BeNull();
    }

    [Fact]
    public async Task DeleteMovieAsync_WithExistingId_ReturnsTrue()
    {
        var id = Guid.NewGuid();
        _repositoryMock.Setup(r => r.DeleteMovieAsync(id)).ReturnsAsync(true);

        var result = await _service.DeleteMovieAsync(id);

        result.Should().BeTrue();
    }

    [Fact]
    public async Task DeleteMovieAsync_WithNonExistingId_ReturnsFalse()
    {
        var id = Guid.NewGuid();
        _repositoryMock.Setup(r => r.DeleteMovieAsync(id)).ReturnsAsync(false);

        var result = await _service.DeleteMovieAsync(id);

        result.Should().BeFalse();
    }
}
