using Microsoft.Extensions.Options;
using Entities = Reservation.API.Domain.Entities;
using Reservation.API.Domain.Enums;
using Reservation.API.Settings;

namespace Reservation.API.Services.Pricing;

public class ReservationFactory(ITicketPricingService pricing, IOptions<ReservationOptions> options) : IReservationFactory
{
    private readonly ITicketPricingService _pricing = pricing ?? throw new ArgumentNullException(nameof(pricing));
    private readonly ReservationOptions _options = options?.Value ?? throw new ArgumentNullException(nameof(options));

    public (Entities.Reservation reservation, List<Entities.Ticket> tickets) CreateReservation(
        Guid id, Guid userId, Guid screeningId, ReservationStatus status,
        IEnumerable<Guid> seatIds)
    {
        var tickets = seatIds.Select(seatId => new Entities.Ticket
        {
            Id = Guid.NewGuid(),
            ReservationId = id,
            SeatId = seatId,
            Price = _pricing.CalculateTicketPrice(),
            QrCode = Guid.NewGuid().ToString()
        }).ToList();

        var reservation = new Entities.Reservation
        {
            Id = id,
            UserId = userId,
            ScreeningId = screeningId,
            Status = status,
            TotalPrice = _pricing.CalculateTotalPrice(tickets.Count),
            ExpiresAt = DateTime.UtcNow.AddMinutes(_options.LockDurationMinutes),
        };

        return (reservation, tickets);
    }
}
