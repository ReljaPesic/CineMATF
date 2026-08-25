using Entities = Reservation.API.Domain.Entities;
using Reservation.API.Domain.Enums;
using Microsoft.EntityFrameworkCore.Storage;

namespace Reservation.API.Repositories;

public interface IReservationRepository
{
    Task<IDbContextTransaction> BeginTransactionAsync();
    Task SaveChangesAsync();
    Task<Entities.SeatLock> LockSeatAsync(Entities.SeatLock seatLock);
    Task<IEnumerable<Entities.SeatLock>> GetActiveLocksByScreeningAsync(Guid screeningId);
    Task<IEnumerable<Entities.SeatLock>> GetActiveLocksBySeatsAsync(Guid screeningId, IEnumerable<Guid> seatIds);
    Task<Entities.Reservation> CreateReservationAsync(Entities.Reservation reservation);
    Task<Entities.Reservation?> GetReservationByIdAsync(Guid id);
    Task<IEnumerable<Entities.Reservation>> GetAllReservationsAsync();
    Task<IEnumerable<Entities.Ticket>> GetAllTicketsAsync();
    Task<Entities.Ticket?> GetTicketByIdAsync(Guid id);
    Task<IEnumerable<Entities.Ticket>> GetTicketsByReservationAsync(Guid reservationId);
    Task<bool> UpdateReservationStatusAsync(Guid id, ReservationStatus status);
    Task<IEnumerable<Entities.Reservation>> GetExpiredReservationsAsync();
    Task<bool> DeleteSeatLockAsync(Guid seatLockId);
    Task DeleteSeatLocksAsync(IEnumerable<Guid> seatLockIds);
    Task<IEnumerable<Entities.Ticket>> CreateTicketsAsync(IEnumerable<Entities.Ticket> tickets);
}
