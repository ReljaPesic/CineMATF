using Entities = Reservation.API.Domain.Entities;
using Reservation.API.ExternalServices;
using Reservation.API.Services.Pricing;

namespace Reservation.API.Services;

public class DataSeeder(
    IServiceProvider serviceProvider,
    IReservationFactory factory,
    ICinemaApiClient cinemaApiClient,
    ITicketPricingService pricingService) : IHostedService
{
    private readonly IServiceProvider _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
    private readonly IReservationFactory _factory = factory ?? throw new ArgumentNullException(nameof(factory));
    private readonly ICinemaApiClient _cinemaApiClient = cinemaApiClient ?? throw new ArgumentNullException(nameof(cinemaApiClient));
    private readonly ITicketPricingService _pricingService = pricingService ?? throw new ArgumentNullException(nameof(pricingService));

    // Cinema/hall ids match the hardcoded seed data in Cinema.API (DataSeeder)
    private static readonly Guid CineMaxCinemaId = new("cccccccc-cccc-cccc-cccc-000000000001");
    private static readonly Guid CineMaxHall1Id = new("bbbbbbbb-bbbb-bbbb-0001-000000000001");
    private static readonly Guid CineMaxHall2Id = new("bbbbbbbb-bbbb-bbbb-0001-000000000002");

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<Data.ReservationDbContext>();

        if (context.Reservations.Any())
            return;

        // Screening ids match the hardcoded seed data in Screening.API (DataSeeder)
        var screeningId1 = new Guid("77777777-7777-7777-7777-000000000001"); // Inception @ CineMax Hall 1
        var screeningId2 = new Guid("77777777-7777-7777-7777-000000000002"); // Interstellar @ CineMax Hall 2

        // Seat ids are not deterministic in Cinema.API, so fetch the real seats for the halls
        // used above instead of hardcoding ids that would never match.
        var hall1Seats = (await _cinemaApiClient.GetSeatsByHallAsync(CineMaxCinemaId, CineMaxHall1Id, cancellationToken))
            .OrderBy(s => s.Row).ThenBy(s => s.Number).ToList();
        var hall2Seats = (await _cinemaApiClient.GetSeatsByHallAsync(CineMaxCinemaId, CineMaxHall2Id, cancellationToken))
            .OrderBy(s => s.Row).ThenBy(s => s.Number).ToList();

        var seat1 = hall1Seats[0];
        var seat2 = hall1Seats[1];
        var seat3 = hall2Seats[0];

        // Confirmed reservations have their tickets generated already (mirrors calling
        // POST api/v1/Ticket/reservation/{reservationId} after confirmation).
        var res1 = _factory.CreateReservation(
            id: new Guid("33333333-3333-3333-3333-333333333333"),
            userId: new Guid("dddddddd-dddd-dddd-dddd-dddddddddddd"),
            screeningId: screeningId1,
            status: Domain.Enums.ReservationStatus.Confirmed,
            seats: [seat1, seat2]);

        res1.Tickets =
        [
            CreateTicket(Guid.Parse("55555555-5555-5555-5555-555555555555"), res1.Id, seat1),
            CreateTicket(new Guid("55555555-5555-5555-5555-555555555556"), res1.Id, seat2)
        ];
        res1.SeatLocks = CreateSeatLocks(res1, [seat1, seat2]);

        // Locked reservations have no tickets yet - those are only created once confirmed.
        var res2 = _factory.CreateReservation(
            Guid.Parse("33333333-3333-3333-3333-333333333334"),
            Guid.Parse("eeeeeeee-eeee-eeee-eeee-eeeeeeeeeeee"),
            screeningId2, Domain.Enums.ReservationStatus.Locked,
            [seat3]);

        res2.SeatLocks = CreateSeatLocks(res2, [seat3]);

        var reservations = new[] { res1, res2 };

        await context.Reservations.AddRangeAsync(reservations, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    // Mirrors the SeatLock creation ReservationService.CreateReservationAsync does for real
    // bookings - without these, the seeded reservations don't actually hold their seats.
    private static List<Entities.SeatLock> CreateSeatLocks(Entities.Reservation reservation, IEnumerable<SeatDetails> seats)
    {
        return seats.Select(seat => new Entities.SeatLock
        {
            Id = Guid.NewGuid(),
            ScreeningId = reservation.ScreeningId,
            SeatId = seat.SeatId,
            UserId = reservation.UserId,
            LockedAt = reservation.CreatedAt,
            ExpiresAt = reservation.ExpiresAt,
            ReservationId = reservation.Id
        }).ToList();
    }

    private Entities.Ticket CreateTicket(Guid id, Guid reservationId, SeatDetails seat) => new()
    {
        Id = id,
        ReservationId = reservationId,
        SeatId = seat.SeatId,
        SeatRow = seat.Row,
        SeatNumber = seat.Number,
        Price = _pricingService.CalculateTicketPrice(seat.SeatType),
        QrCode = Guid.NewGuid().ToString()
    };
}
