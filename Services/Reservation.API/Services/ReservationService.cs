using AutoMapper;
using Entities = Reservation.API.Domain.Entities;
using Reservation.API.Domain.Enums;
using Reservation.API.DTOs.Requests;
using Reservation.API.DTOs.Responses;
using Reservation.API.Repositories;
using Reservation.API.Services.Pricing;

namespace Reservation.API.Services;

public class ReservationService(IReservationRepository repository, IMapper mapper, IReservationFactory factory) : IReservationService
{
    private readonly IReservationRepository _repository = repository ?? throw new ArgumentNullException(nameof(repository));
    private readonly IMapper _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
    private readonly IReservationFactory _factory = factory ?? throw new ArgumentNullException(nameof(factory));

    public async Task<AvailableSeatsResponse> GetAvailableSeatsAsync(Guid screeningId)
    {
        var activeLocks = await _repository.GetActiveLocksByScreeningAsync(screeningId);
        var lockedSeats = _mapper.Map<IEnumerable<SeatLockResponse>>(activeLocks);

        return new AvailableSeatsResponse(screeningId, [], lockedSeats);
    }

    public async Task<(bool Success, string? ErrorMessage, ReservationResponse? Response)> CreateReservationAsync(CreateReservationRequest request)
    {
        var existingLocks = await _repository.GetActiveLocksBySeatsAsync(request.ScreeningId, request.SeatIds);
        if (existingLocks.Any())
        {
            var lockedSeatIds = existingLocks.Select(l => l.SeatId).ToList();
            return (false, $"Some seats are already locked: {string.Join(", ", lockedSeatIds)}", null);
        }

        var (reservation, tickets) = _factory.CreateReservation(
            Guid.NewGuid(), request.UserId, request.ScreeningId,
            ReservationStatus.Locked, request.SeatIds);

        await using var transaction = await _repository.BeginTransactionAsync();
        try
        {
            var createdReservation = await _repository.CreateReservationAsync(reservation);

            foreach (var seatId in request.SeatIds)
            {
                var seatLock = new Entities.SeatLock
                {
                    Id = Guid.NewGuid(),
                    ScreeningId = request.ScreeningId,
                    SeatId = seatId,
                    UserId = request.UserId,
                    LockedAt = DateTime.UtcNow,
                    ExpiresAt = DateTime.UtcNow.AddMinutes(10),
                    ReservationId = createdReservation.Id
                };
                await _repository.LockSeatAsync(seatLock);
            }

            await _repository.CreateTicketsAsync(tickets);

            await transaction.CommitAsync();

            var response = _mapper.Map<ReservationResponse>(createdReservation);
            return (true, null, response);
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task<ReservationResponse?> GetReservationByIdAsync(Guid id)
    {
        var reservation = await _repository.GetReservationByIdAsync(id);
        return reservation == null ? null : _mapper.Map<ReservationResponse>(reservation);
    }

    public async Task<IEnumerable<ReservationResponse>> GetAllReservationsAsync()
    {
        var reservations = await _repository.GetAllReservationsAsync();
        return _mapper.Map<IEnumerable<ReservationResponse>>(reservations);
    }

    public async Task<IEnumerable<TicketResponse>> GetAllTicketsAsync()
    {
        var tickets = await _repository.GetAllTicketsAsync();
        return _mapper.Map<IEnumerable<TicketResponse>>(tickets);
    }

    public async Task<TicketResponse?> GetTicketByIdAsync(Guid id)
    {
        var ticket = await _repository.GetTicketByIdAsync(id);
        return ticket == null ? null : _mapper.Map<TicketResponse>(ticket);
    }

    public async Task<IEnumerable<TicketResponse>> GetReservationTicketsAsync(Guid reservationId)
    {
        var tickets = await _repository.GetTicketsByReservationAsync(reservationId);
        return _mapper.Map<IEnumerable<TicketResponse>>(tickets);
    }

    //this is temporarly testing too until payment service
    public async Task<(bool Success, string? ErrorMessage)> PayAsync(Guid reservationId)
    {
        var reservation = await _repository.GetReservationByIdAsync(reservationId);
        if (reservation == null) return (false, "Reservation not found");

        if (reservation.Status != ReservationStatus.Locked)
            return (false, "Only locked reservations can initiate payment");

        await _repository.UpdateReservationStatusAsync(reservationId, ReservationStatus.Pending);
        return (true, null);
    }

    //this one too
    public async Task<(bool Success, string? ErrorMessage)> ConfirmReservationAsync(Guid reservationId, Guid paymentId)
    {
        var reservation = await _repository.GetReservationByIdAsync(reservationId);
        if (reservation == null) return (false, "Reservation not found");

        if (reservation.Status != ReservationStatus.Pending)
            return (false, "Only pending reservations can be confirmed");

        await _repository.UpdateReservationStatusAsync(reservationId, ReservationStatus.Confirmed);
        return (true, null);
    }

    public async Task<bool> CancelReservationAsync(Guid id)
    {
        var reservation = await _repository.GetReservationByIdAsync(id);
        if (reservation == null) return false;

        await _repository.UpdateReservationStatusAsync(id, ReservationStatus.Cancelled);

        foreach (var seatLock in reservation.SeatLocks)
        {
            await _repository.DeleteSeatLockAsync(seatLock.Id);
        }

        return true;
    }
}
