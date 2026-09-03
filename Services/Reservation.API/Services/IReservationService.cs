using Reservation.API.DTOs.Requests;
using Reservation.API.DTOs.Responses;

namespace Reservation.API.Services;

public interface IReservationService
{
    Task<AvailableSeatsResponse?> GetAvailableSeatsAsync(Guid screeningId);
    Task<(bool Success, string? ErrorMessage, ReservationResponse? Response)> CreateReservationAsync(CreateReservationRequest request);
    Task<ReservationResponse?> GetReservationByIdAsync(Guid id);
    Task<IEnumerable<ReservationResponse>> GetAllReservationsAsync();
    Task<IEnumerable<ReservationResponse>> GetReservationsByUserIdAsync(Guid userId);
    Task<IEnumerable<TicketResponse>> GetAllTicketsAsync();
    Task<TicketResponse?> GetTicketByIdAsync(Guid id);
    Task<IEnumerable<TicketResponse>> GetReservationTicketsAsync(Guid reservationId);
    Task<(bool Success, string? ErrorMessage, byte[]? Content, string? FileName)> GetTicketFileAsync(Guid ticketId);
    Task<(bool Success, string? ErrorMessage)> PayAsync(Guid reservationId);
    Task<(bool Success, string? ErrorMessage, IEnumerable<TicketResponse>? Tickets)> GenerateTicketsAsync(Guid reservationId);
    Task<(bool Success, string? ErrorMessage)> CancelReservationAsync(Guid id);
    Task ExpireReservationAsync(Guid id);
}
