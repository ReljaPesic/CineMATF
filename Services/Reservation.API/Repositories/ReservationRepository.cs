using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Reservation.API.Data;
using Entities = Reservation.API.Domain.Entities;
using Reservation.API.Domain.Enums;

namespace Reservation.API.Repositories;

public class ReservationRepository(ReservationDbContext context) : IReservationRepository
{
    private readonly ReservationDbContext _context = context ?? throw new ArgumentNullException(nameof(context));

    public async Task<IDbContextTransaction> BeginTransactionAsync()
    {
        return await _context.Database.BeginTransactionAsync();
    }

    public async Task<Entities.SeatLock> LockSeatAsync(Entities.SeatLock seatLock)
    {
        await _context.SeatLocks.AddAsync(seatLock);
        await _context.SaveChangesAsync();
        return seatLock;
    }

    public async Task<IEnumerable<Entities.SeatLock>> GetActiveLocksByScreeningAsync(Guid screeningId)
    {
        return await _context.SeatLocks
            .AsNoTracking()
            .Where(s => s.ScreeningId == screeningId && s.ExpiresAt > DateTime.UtcNow && s.ReservationId == null)
            .ToListAsync();
    }

    public async Task<IEnumerable<Entities.SeatLock>> GetActiveLocksBySeatsAsync(Guid screeningId, IEnumerable<Guid> seatIds)
    {
        return await _context.SeatLocks
            .AsNoTracking()
            .Where(s => s.ScreeningId == screeningId && seatIds.Contains(s.SeatId) && s.ExpiresAt > DateTime.UtcNow && s.ReservationId == null)
            .ToListAsync();
    }

    public async Task<Entities.Reservation> CreateReservationAsync(Entities.Reservation reservation)
    {
        await _context.Reservations.AddAsync(reservation);
        await _context.SaveChangesAsync();
        return reservation;
    }

    public async Task<Entities.Reservation?> GetReservationByIdAsync(Guid id)
    {
        return await _context.Reservations
            .AsNoTracking()
            .Include(r => r.Tickets)
            .Include(r => r.SeatLocks)
            .FirstOrDefaultAsync(r => r.Id == id);
    }

    public async Task<IEnumerable<Entities.Reservation>> GetAllReservationsAsync()
    {
        return await _context.Reservations
            .AsNoTracking()
            .Include(r => r.Tickets)
            .Include(r => r.SeatLocks)
            .ToListAsync();
    }

    public async Task<IEnumerable<Entities.Ticket>> GetAllTicketsAsync()
    {
        return await _context.Tickets
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<Entities.Ticket?> GetTicketByIdAsync(Guid id)
    {
        return await _context.Tickets
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.Id == id);
    }

    public async Task<IEnumerable<Entities.Ticket>> GetTicketsByReservationAsync(Guid reservationId)
    {
        return await _context.Tickets
            .AsNoTracking()
            .Where(t => t.ReservationId == reservationId)
            .ToListAsync();
    }

    public async Task<bool> UpdateReservationStatusAsync(Guid id, ReservationStatus status)
    {
        var reservation = await _context.Reservations.FindAsync(id);
        if (reservation == null) return false;

        reservation.Status = status;
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task CleanExpiredLocksAsync()
    {
        var expiredLocks = await _context.SeatLocks
            .Where(s => s.ExpiresAt <= DateTime.UtcNow && s.ReservationId == null)
            .ToListAsync();

        if (expiredLocks.Any())
        {
            _context.SeatLocks.RemoveRange(expiredLocks);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<IEnumerable<Entities.Reservation>> GetExpiredReservationsAsync()
    {
        return await _context.Reservations
            .Include(r => r.SeatLocks)
            .Where(r => (r.Status == ReservationStatus.Locked || r.Status == ReservationStatus.Pending)
                        && r.ExpiresAt <= DateTime.UtcNow)
            .ToListAsync();
    }

    public async Task<bool> DeleteSeatLockAsync(Guid seatLockId)
    {
        var seatLock = await _context.SeatLocks.FindAsync(seatLockId);
        if (seatLock == null) return false;

        _context.SeatLocks.Remove(seatLock);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<IEnumerable<Entities.Ticket>> CreateTicketsAsync(IEnumerable<Entities.Ticket> tickets)
    {
        await _context.Tickets.AddRangeAsync(tickets);
        await _context.SaveChangesAsync();
        return tickets;
    }
}
