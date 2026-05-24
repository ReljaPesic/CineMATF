using Entities = Reservation.API.Domain.Entities;
using Reservation.API.Domain.Enums;
using Microsoft.EntityFrameworkCore.Storage;

namespace Reservation.API.Repositories;

public interface IReservationRepository
{
    Task<IDbContextTransaction> BeginTransactionAsync();
    Task<Entities.SeatLock> LockSeatAsync(Entities.SeatLock seatLock);
    Task<IEnumerable<Entities.SeatLock>> GetActiveLocksByScreeningAsync(Guid screeningId);
    Task<IEnumerable<Entities.SeatLock>> GetActiveLocksBySeatsAsync(Guid screeningId, IEnumerable<Guid> seatIds);
    Task<Entities.Reservation> CreateReservationAsync(Entities.Reservation reservation);
    Task<Entities.Reservation?> GetReservationByIdAsync(Guid id);
    Task<IEnumerable<Entities.Reservation>> GetAllReservationsAsync();
    Task<IEnumerable<Entities.Ticket>> GetAllTicketsAsync();
    Task<bool> UpdateReservationStatusAsync(Guid id, ReservationStatus status);
    Task CleanExpiredLocksAsync();
    Task<IEnumerable<Entities.Reservation>> GetExpiredReservationsAsync();
    Task<bool> DeleteSeatLockAsync(Guid seatLockId);
    Task<IEnumerable<Entities.Ticket>> CreateTicketsAsync(IEnumerable<Entities.Ticket> tickets);
}
