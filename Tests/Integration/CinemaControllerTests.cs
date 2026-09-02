using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Cinema.API.Authorization;
using Cinema.API.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Cinema.API.Tests.Integration;

public class CinemaControllerTests(CinemaApiFactory factory) : IClassFixture<CinemaApiFactory>
{
    private readonly HttpClient _client = factory.CreateClient();

    private void AuthenticateAs(string role) =>
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", TestJwt.CreateFor(role));

    [Fact]
    public async Task GetCinemas_ReturnsEmptyList_WhenNoCinemas()
    {
        var response = await _client.GetAsync("/api/v1/cinema");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetCinemaById_Returns404_WhenNotFound()
    {
        var id = Guid.NewGuid();

        var response = await _client.GetAsync($"/api/v1/cinema/{id}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task CreateCinema_ReturnsCreated_WhenValidRequest()
    {
        AuthenticateAs(Roles.Admin);
        var request = new { name = "CineMax", city = "Beograd" };

        var response = await _client.PostAsJsonAsync("/api/v1/cinema", request);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
    }

    [Fact]
    public async Task CreateCinema_ReturnsBadRequest_WhenInvalidName()
    {
        AuthenticateAs(Roles.Admin);
        var request = new { name = "", city = "Beograd" };

        var response = await _client.PostAsJsonAsync("/api/v1/cinema", request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task CreateCinema_ReturnsUnauthorized_WhenNoToken()
    {
        var request = new { name = "CineMax", city = "Beograd" };

        var response = await _client.PostAsJsonAsync("/api/v1/cinema", request);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task CreateCinema_ReturnsForbidden_WhenCallerIsNotAdmin()
    {
        AuthenticateAs("User");
        var request = new { name = "CineMax", city = "Beograd" };

        var response = await _client.PostAsJsonAsync("/api/v1/cinema", request);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task UpdateCinema_ReturnsNotFound_WhenNotExists()
    {
        AuthenticateAs(Roles.Admin);
        var id = Guid.NewGuid();
        var request = new { name = "Updated Cinema", city = "Beograd" };

        var response = await _client.PutAsJsonAsync($"/api/v1/cinema/{id}", request);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task UpdateCinema_ReturnsForbidden_WhenCallerIsNotAdmin()
    {
        AuthenticateAs("User");
        var id = Guid.NewGuid();
        var request = new { name = "Updated Cinema", city = "Beograd" };

        var response = await _client.PutAsJsonAsync($"/api/v1/cinema/{id}", request);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task DeleteCinema_ReturnsNotFound_WhenNotExists()
    {
        AuthenticateAs(Roles.Admin);
        var id = Guid.NewGuid();

        var response = await _client.DeleteAsync($"/api/v1/cinema/{id}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task DeleteCinema_ReturnsForbidden_WhenCallerIsNotAdmin()
    {
        AuthenticateAs("User");
        var id = Guid.NewGuid();

        var response = await _client.DeleteAsync($"/api/v1/cinema/{id}");

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task GetCinemasByCity_Returns400_WhenInvalidCityName()
    {
        var response = await _client.GetAsync("/api/v1/cinema/city/InvalidCity");

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}

public class CinemaApiFactory : WebApplicationFactory<Program>
{
    private readonly string _dbName = $"TestDb_{Guid.NewGuid():N}";

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureServices(services =>
        {
            var allDescriptors = services.ToList();
            foreach (var descriptor in allDescriptors)
            {
                var serviceType = descriptor.ServiceType;
                var implType = descriptor.ImplementationType;

                if (implType == typeof(CinemaDbContext) ||
                    serviceType == typeof(DbContextOptions<CinemaDbContext>) ||
                    serviceType == typeof(ICinemaRepository) ||
                    serviceType == typeof(ICinemaService))
                {
                    services.Remove(descriptor);
                    continue;
                }

                if (implType?.Assembly.FullName?.Contains("Npgsql") == true ||
                    implType?.Assembly.FullName?.Contains("Microsoft.EntityFrameworkCore") == true)
                {
                    if (serviceType.FullName?.Contains("Npgsql") == true ||
                        serviceType.FullName?.Contains("Relational") == true)
                    {
                        services.Remove(descriptor);
                    }
                }
            }

            var newOptions = new DbContextOptionsBuilder<CinemaDbContext>()
                .UseInMemoryDatabase(_dbName)
                .Options;

            services.AddSingleton(newOptions);
            services.AddDbContext<CinemaDbContext>();

            services.AddScoped<ICinemaRepository, CinemaRepository>();
            services.AddScoped<ICinemaService, CinemaService>();
        });
    }
}
