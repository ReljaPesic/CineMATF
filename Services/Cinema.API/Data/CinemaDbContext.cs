using Cinema.API.Entities;
using Microsoft.EntityFrameworkCore;

namespace Cinema.API.Data;

public class CinemaDbContext(DbContextOptions<CinemaDbContext> options) : DbContext(options)
{
    public DbSet<MovieTheatre> MovieTheatres { get; set; }
    public DbSet<Hall> Halls { get; set; }
    public DbSet<Seat> Seats { get; set; }
}
