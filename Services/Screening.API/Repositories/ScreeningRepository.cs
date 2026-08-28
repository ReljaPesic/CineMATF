using Dapper;
using Screening.API.Data;
using Entities = Screening.API.Entities;

namespace Screening.API.Repositories;

public class ScreeningRepository(IScreeningContext context) : IScreeningRepository
{
    private readonly IScreeningContext _context = context ?? throw new ArgumentNullException(nameof(context));

    public async Task<IEnumerable<Entities.Screening>> GetScreeningsAsync(Guid? movieId, DateOnly? date, Guid? cinemaId)
    {
        const string sql = """
            SELECT id AS "Id", movieid AS "MovieId", hallid AS "HallId", cinemaid AS "CinemaId",
                   starttime AS "StartTime", format AS "Format", status AS "Status"
            FROM screenings
            WHERE (@MovieId::uuid IS NULL OR movieid = @MovieId::uuid)
              AND (@CinemaId::uuid IS NULL OR cinemaid = @CinemaId::uuid)
              AND (@Date::date IS NULL OR starttime::date = @Date::date)
            ORDER BY starttime
            """;

        using var connection = _context.GetConnection();
        return await connection.QueryAsync<Entities.Screening>(sql, new
        {
            MovieId = movieId,
            CinemaId = cinemaId,
            Date = date.HasValue ? date.Value.ToDateTime(TimeOnly.MinValue) : (DateTime?)null
        });
    }

    public async Task<Entities.Screening?> GetScreeningByIdAsync(Guid id)
    {
        const string sql = """
            SELECT id AS "Id", movieid AS "MovieId", hallid AS "HallId", cinemaid AS "CinemaId",
                   starttime AS "StartTime", format AS "Format", status AS "Status"
            FROM screenings
            WHERE id = @Id
            """;

        using var connection = _context.GetConnection();
        return await connection.QuerySingleOrDefaultAsync<Entities.Screening>(sql, new { Id = id });
    }

    public async Task<Entities.Screening> CreateScreeningAsync(Entities.Screening screening)
    {
        screening.Id = Guid.NewGuid();

        const string sql = """
            INSERT INTO screenings (id, movieid, hallid, cinemaid, starttime, format, status)
            VALUES (@Id, @MovieId, @HallId, @CinemaId, @StartTime, @Format, @Status)
            """;

        using var connection = _context.GetConnection();
        await connection.ExecuteAsync(sql, screening);
        return screening;
    }

    public async Task<bool> UpdateScreeningAsync(Entities.Screening screening)
    {
        const string sql = """
            UPDATE screenings
            SET movieid = @MovieId, hallid = @HallId, cinemaid = @CinemaId,
                starttime = @StartTime, format = @Format, status = @Status
            WHERE id = @Id
            """;

        using var connection = _context.GetConnection();
        var affected = await connection.ExecuteAsync(sql, screening);
        return affected > 0;
    }

    public async Task<bool> DeleteScreeningAsync(Guid id)
    {
        const string sql = "DELETE FROM screenings WHERE id = @Id";

        using var connection = _context.GetConnection();
        var affected = await connection.ExecuteAsync(sql, new { Id = id });
        return affected > 0;
    }
}
