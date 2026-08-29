using Dapper;

namespace Screening.API.Data;

public static class ScreeningSchemaInitializer
{
    private const string CreateTableSql = """
        CREATE TABLE IF NOT EXISTS screenings (
            id UUID PRIMARY KEY,
            movieid UUID NOT NULL,
            hallid UUID NOT NULL,
            cinemaid UUID NOT NULL,
            starttime TIMESTAMP NOT NULL,
            format INTEGER NOT NULL
        );
        """;

    public static async Task EnsureCreatedAsync(this IApplicationBuilder app)
    {
        using var scope = app.ApplicationServices.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<IScreeningContext>();

        using var connection = context.GetConnection();
        await connection.ExecuteAsync(CreateTableSql);
    }
}
