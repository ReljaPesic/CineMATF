using AutoMapper;
using Grpc.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Entities = Reservation.API.Domain.Entities;
using Reservation.API.Domain.Enums;
using Reservation.API.DTOs.Requests;
using Reservation.API.DTOs.Responses;
using Reservation.API.ExternalServices;
using Reservation.API.Settings;
using Reservation.API.Repositories;
using Reservation.API.Services.Pricing;

namespace Reservation.API.Services;

public class ReservationService(
    IReservationRepository repository,
    IMapper mapper,
    IReservationFactory factory,
    IOptions<ReservationOptions> options,
    ICinemaApiClient cinemaApiClient,
    IScreeningApiClient screeningApiClient) : IReservationService
{
    private readonly IReservationRepository _repository = repository ?? throw new ArgumentNullException(nameof(repository));
    private readonly IMapper _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
    private readonly IReservationFactory _factory = factory ?? throw new ArgumentNullException(nameof(factory));
    private readonly ReservationOptions _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
    private readonly ICinemaApiClient _cinemaApiClient = cinemaApiClient ?? throw new ArgumentNullException(nameof(cinemaApiClient));
    private readonly IScreeningApiClient _screeningApiClient = screeningApiClient ?? throw new ArgumentNullException(nameof(screeningApiClient));

    public async Task<AvailableSeatsResponse?> GetAvailableSeatsAsync(Guid screeningId)
    {
        var screening = await _screeningApiClient.GetScreeningAsync(screeningId);
        if (screening == null)
        {
            return null;
        }

        var allSeats = await _cinemaApiClient.GetSeatsByHallAsync(screening.CinemaId, screening.HallId);
        var activeLocks = await _repository.GetActiveLocksByScreeningAsync(screeningId);
        var lockedSeatIds = activeLocks.Select(l => l.SeatId).ToHashSet();

        var availableSeats = allSeats.Select(s => s.SeatId).Where(seatId => !lockedSeatIds.Contains(seatId));
        var lockedSeats = _mapper.Map<IEnumerable<SeatLockResponse>>(activeLocks);

        return new AvailableSeatsResponse(screeningId, availableSeats, lockedSeats);
    }

    public async Task<(bool Success, string? ErrorMessage, ReservationResponse? Response)> CreateReservationAsync(CreateReservationRequest request)
    {
        if (request.UserId == Guid.Empty)
        {
            return (false, "UserId must be provided", null);
        }

        var duplicateSeatIds = request.SeatIds
            .GroupBy(seatId => seatId)
            .Where(group => group.Count() > 1)
            .Select(group => group.Key)
            .ToList();
        if (duplicateSeatIds.Count != 0)
        {
            return (false, $"Duplicate seat ids in request: {string.Join(", ", duplicateSeatIds)}", null);
        }

        ScreeningDetails? screening;
        try
        {
            screening = await _screeningApiClient.GetScreeningAsync(request.ScreeningId);
        }
        catch (RpcException)
        {
            return (false, "Unable to verify screening right now, please try again later", null);
        }

        if (screening == null)
        {
            return (false, "Screening not found", null);
        }
        if (screening.StartTime <= DateTime.UtcNow)
        {
            return (false, "Screening has already started", null);
        }

        var existingLocks = await _repository.GetActiveLocksBySeatsAsync(request.ScreeningId, request.SeatIds);
        if (existingLocks.Any())
        {
            var lockedSeatIds = existingLocks.Select(l => l.SeatId).ToList();
            return (false, $"Some seats are already locked: {string.Join(", ", lockedSeatIds)}", null);
        }

        var seats = new List<SeatDetails>();
        try
        {
            foreach (var seatId in request.SeatIds)
            {
                var seat = await _cinemaApiClient.GetSeatAsync(seatId);
                if (seat == null)
                {
                    return (false, $"Seat {seatId} does not exist", null);
                }
                seats.Add(seat);
            }
        }
        catch (HttpRequestException)
        {
            return (false, "Unable to verify seats right now, please try again later", null);
        }

        var (reservation, tickets) = _factory.CreateReservation(
            Guid.NewGuid(), request.UserId, request.ScreeningId,
            ReservationStatus.Locked, seats);

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
                    ExpiresAt = DateTime.UtcNow.AddMinutes(_options.LockDurationMinutes),
                    ReservationId = createdReservation.Id
                };
                await _repository.LockSeatAsync(seatLock);
            }

            await _repository.CreateTicketsAsync(tickets);

            await _repository.SaveChangesAsync();
            await transaction.CommitAsync();

            var response = _mapper.Map<ReservationResponse>(createdReservation);
            return (true, null, response);
        }
        catch (DbUpdateException ex) when (IsUniqueConstraintViolation(ex))
        {
            await transaction.RollbackAsync();
            return (false, "Some seats are no longer available", null);
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    private static bool IsUniqueConstraintViolation(DbUpdateException ex)
    {
        return ex.InnerException?.Message.Contains("unique", StringComparison.OrdinalIgnoreCase) == true
            || ex.InnerException?.Message.Contains("duplicate key", StringComparison.OrdinalIgnoreCase) == true;
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

    public async Task<(bool Success, string? ErrorMessage)> PayAsync(Guid reservationId)
    {
        var reservation = await _repository.GetReservationByIdAsync(reservationId);
        if (reservation == null) return (false, "Reservation not found");

        if (reservation.Status != ReservationStatus.Locked)
            return (false, "Only locked reservations can initiate payment");

        if (reservation.ExpiresAt <= DateTime.UtcNow)
            return (false, "Reservation has expired");

        await _repository.UpdateReservationStatusAsync(reservationId, ReservationStatus.Pending);
        return (true, null);
    }

    public async Task<(bool Success, string? ErrorMessage)> ConfirmReservationAsync(Guid reservationId, Guid paymentId)
    {
        var reservation = await _repository.GetReservationByIdAsync(reservationId);
        if (reservation == null) return (false, "Reservation not found");

        if (reservation.Status != ReservationStatus.Pending)
            return (false, "Only pending reservations can be confirmed");

        if (reservation.ExpiresAt <= DateTime.UtcNow)
            return (false, "Reservation has expired");

        await _repository.UpdateReservationStatusAsync(reservationId, ReservationStatus.Confirmed);
        return (true, null);
    }

    public async Task<bool> CancelReservationAsync(Guid id)
    {
        var reservation = await _repository.GetReservationByIdAsync(id);
        if (reservation == null) return false;

        await using var transaction = await _repository.BeginTransactionAsync();
        try
        {
            await _repository.UpdateReservationStatusAsync(id, ReservationStatus.Cancelled);
            await _repository.DeleteSeatLocksAsync(reservation.SeatLocks.Select(sl => sl.Id));
            await transaction.CommitAsync();
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }

        return true;
    }

    public async Task ExpireReservationAsync(Guid id)
    {
        var reservation = await _repository.GetReservationByIdAsync(id);
        if (reservation == null) return;

        await using var transaction = await _repository.BeginTransactionAsync();
        try
        {
            await _repository.UpdateReservationStatusAsync(id, ReservationStatus.Expired);
            await _repository.DeleteSeatLocksAsync(reservation.SeatLocks.Select(sl => sl.Id));
            await transaction.CommitAsync();
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }
}
