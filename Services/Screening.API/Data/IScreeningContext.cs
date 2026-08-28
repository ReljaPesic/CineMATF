using Npgsql;

namespace Screening.API.Data
{
    public interface IScreeningContext
    {
        NpgsqlConnection GetConnection();
    }
}