using Npgsql;

namespace Screening.API.Data;

public class ScreeningDbContext : IScreeningContext
{
    private readonly IConfiguration _configuration;

    public ScreeningDbContext(IConfiguration configuration)
    {
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
    }
    public NpgsqlConnection GetConnection()
    {
        return new NpgsqlConnection(_configuration.GetConnectionString("Default"));
    }
}
