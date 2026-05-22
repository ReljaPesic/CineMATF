using Reservation.API.DTOs.Requests;
using Reservation.API.DTOs.Responses;

namespace Reservation.API.Services;

public interface IReservationService
{
    Task<(bool Success, string? ErrorMessage, IEnumerable<SeatLockResponse>? LockedSeats)> LockSeatsAsync(LockSeatsRequest request);
    Task<AvailableSeatsResponse> GetAvailableSeatsAsync(Guid screeningId);
    Task<(bool Success, string? ErrorMessage, ReservationResponse? Response)> CreateReservationAsync(CreateReservationRequest request);
    Task<ReservationResponse?> GetReservationByIdAsync(Guid id);
    Task<IEnumerable<ReservationResponse>> GetAllReservationsAsync();
    Task<IEnumerable<TicketResponse>> GetAllTicketsAsync();
    Task<(bool Success, string? ErrorMessage)> InitiatePaymentAsync(Guid reservationId);
    Task<(bool Success, string? ErrorMessage)> ConfirmReservationAsync(Guid reservationId, Guid paymentId);
    Task<bool> CancelReservationAsync(Guid id);
}
