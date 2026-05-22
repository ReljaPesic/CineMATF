using AutoMapper;
using Entities = Reservation.API.Domain.Entities;
using Reservation.API.Domain.Enums;
using Reservation.API.DTOs.Requests;
using Reservation.API.DTOs.Responses;
using Reservation.API.Repositories;

namespace Reservation.API.Services;

public class ReservationService(IReservationRepository repository, IMapper mapper) : IReservationService
{
    private readonly IReservationRepository _repository = repository ?? throw new ArgumentNullException(nameof(repository));
    private readonly IMapper _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));

    public async Task<(bool Success, string? ErrorMessage, IEnumerable<SeatLockResponse>? LockedSeats)> LockSeatsAsync(LockSeatsRequest request)
    {
        var existingLocks = await _repository.GetActiveLocksBySeatsAsync(request.ScreeningId, request.SeatIds);
        if (existingLocks.Any())
        {
            var lockedSeatIds = existingLocks.Select(l => l.SeatId).ToList();
            return (false, $"Seats already locked: {string.Join(", ", lockedSeatIds)}", null);
        }

        var lockedSeats = new List<SeatLockResponse>();
        foreach (var seatId in request.SeatIds)
        {
            var seatLock = new Entities.SeatLock
            {
                Id = Guid.NewGuid(),
                ScreeningId = request.ScreeningId,
                SeatId = seatId,
                UserId = request.UserId,
                LockedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddMinutes(10)
            };
            var created = await _repository.LockSeatAsync(seatLock);
            lockedSeats.Add(new SeatLockResponse(created.SeatId, created.LockedAt, created.ExpiresAt));
        }

        return (true, null, lockedSeats);
    }

    public async Task<AvailableSeatsResponse> GetAvailableSeatsAsync(Guid screeningId)
    {
        var activeLocks = await _repository.GetActiveLocksByScreeningAsync(screeningId);
        var lockedSeats = _mapper.Map<IEnumerable<SeatLockResponse>>(activeLocks);

        return new AvailableSeatsResponse(screeningId, [], lockedSeats);
    }

    public async Task<(bool Success, string? ErrorMessage, ReservationResponse? Response)> CreateReservationAsync(CreateReservationRequest request)
    {
        var existingLocks = await _repository.GetActiveLocksBySeatsAsync(request.ScreeningId, request.SeatIds);
        if (!existingLocks.Any() || existingLocks.Any(l => l.UserId != request.UserId))
        {
            return (false, "Seats are not locked by this user or locks expired", null);
        }

        var reservation = new Entities.Reservation
        {
            Id = Guid.NewGuid(),
            UserId = request.UserId,
            ScreeningId = request.ScreeningId,
            Status = ReservationStatus.Locked,
            TotalPrice = request.SeatIds.Count() * 10.0m,
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddMinutes(15)
        };

        var createdReservation = await _repository.CreateReservationAsync(reservation);

        foreach (var seatLock in existingLocks)
        {
            seatLock.ReservationId = createdReservation.Id;
        }

        var tickets = new List<Entities.Ticket>();
        foreach (var seatId in request.SeatIds)
        {
            tickets.Add(new Entities.Ticket
            {
                Id = Guid.NewGuid(),
                ReservationId = createdReservation.Id,
                SeatId = seatId,
                Price = 10.0m,
                QrCode = Guid.NewGuid().ToString()
            });
        }

        await _repository.CreateTicketsAsync(tickets);
        var response = _mapper.Map<ReservationResponse>(createdReservation);
        return (true, null, response);
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

    public async Task<(bool Success, string? ErrorMessage)> InitiatePaymentAsync(Guid reservationId)
    {
        var reservation = await _repository.GetReservationByIdAsync(reservationId);
        if (reservation == null) return (false, "Reservation not found");

        if (reservation.Status != ReservationStatus.Locked)
            return (false, "Only locked reservations can initiate payment");

        await _repository.UpdateReservationStatusAsync(reservationId, ReservationStatus.Pending);
        return (true, null);
    }

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
