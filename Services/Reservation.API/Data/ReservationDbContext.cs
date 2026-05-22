using Microsoft.EntityFrameworkCore;
using Entities = Reservation.API.Domain.Entities;

namespace Reservation.API.Data;

public class ReservationDbContext(DbContextOptions<ReservationDbContext> options) : DbContext(options)
{
    public DbSet<Entities.Reservation> Reservations { get; set; }
    public DbSet<Entities.SeatLock> SeatLocks { get; set; }
    public DbSet<Entities.Ticket> Tickets { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Entities.SeatLock>()
            .HasIndex(s => new { s.ScreeningId, s.SeatId });

        modelBuilder.Entity<Entities.SeatLock>()
            .HasIndex(s => s.ExpiresAt);

        modelBuilder.Entity<Entities.Reservation>()
            .HasIndex(r => r.Status);

        modelBuilder.Entity<Entities.Reservation>()
            .Property(r => r.Status)
            .HasConversion<string>();

        modelBuilder.Entity<Entities.Ticket>()
            .HasIndex(t => t.QrCode)
            .IsUnique();
    }
}
