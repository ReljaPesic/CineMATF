using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace Identity.API.Data;

// Used only by the EF Core command-line tools (`dotnet ef migrations add ...`,
// `dotnet ef database update`). It builds an IdentityContext WITHOUT starting
// the web host, reading the same connection-string key the running app uses
// (appsettings.json / appsettings.Development.json, or an env var override).
public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<IdentityContext>
{
    public IdentityContext CreateDbContext(string[] args)
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: true)
            .AddJsonFile("appsettings.Development.json", optional: true)
            .AddEnvironmentVariables()
            .Build();

        var connectionString = configuration.GetConnectionString("IdentityConnectionString")
            ?? "Host=localhost;Port=5436;Database=IdentityDB;Username=identity;Password=identity";

        var optionsBuilder = new DbContextOptionsBuilder<IdentityContext>();
        optionsBuilder.UseNpgsql(connectionString);

        return new IdentityContext(optionsBuilder.Options);
    }
}
