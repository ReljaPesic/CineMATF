using Cinema.API.Entities;
using Microsoft.EntityFrameworkCore;

namespace Cinema.API.Data;

public class CinemaDbContext(DbContextOptions<CinemaDbContext> options) : DbContext(options)
{
    public DbSet<MovieTheatre> MovieTheatres { get; set; }
    public DbSet<Hall> Halls { get; set; }
    public DbSet<Seat> Seats { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Seat>()
            .HasIndex(s => new { s.HallId, s.Row, s.Number });
        modelBuilder.Entity<MovieTheatre>()
            .HasIndex(c => c.City);
        modelBuilder.Entity<Hall>()
            .HasIndex(h => new { h.CinemaId, h.Name }).IsUnique();
    }
}
