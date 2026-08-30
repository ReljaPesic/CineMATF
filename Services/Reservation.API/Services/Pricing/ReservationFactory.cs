using Microsoft.Extensions.Options;
using Entities = Reservation.API.Domain.Entities;
using Reservation.API.Domain.Enums;
using Reservation.API.ExternalServices;
using Reservation.API.Settings;

namespace Reservation.API.Services.Pricing;

public class ReservationFactory(ITicketPricingService pricing, IOptions<ReservationOptions> options) : IReservationFactory
{
    private readonly ITicketPricingService _pricing = pricing ?? throw new ArgumentNullException(nameof(pricing));
    private readonly ReservationOptions _options = options?.Value ?? throw new ArgumentNullException(nameof(options));

    public Entities.Reservation CreateReservation(
        Guid id, Guid userId, Guid screeningId, ReservationStatus status,
        IEnumerable<SeatDetails> seats)
    {
        var seatList = seats.ToList();

        return new Entities.Reservation
        {
            Id = id,
            UserId = userId,
            ScreeningId = screeningId,
            Status = status,
            TotalPrice = _pricing.CalculateTotalPrice(seatList.Select(seat => seat.SeatType)),
            ExpiresAt = DateTime.UtcNow.AddMinutes(_options.LockDurationMinutes),
        };
    }
}
