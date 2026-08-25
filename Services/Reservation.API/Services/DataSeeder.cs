using Entities = Reservation.API.Domain.Entities;
using Reservation.API.ExternalServices;
using Reservation.API.Services.Pricing;

namespace Reservation.API.Services;

public class DataSeeder(IServiceProvider serviceProvider, IReservationFactory factory) : IHostedService
{
    private readonly IServiceProvider _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
    private readonly IReservationFactory _factory = factory ?? throw new ArgumentNullException(nameof(factory));

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<Data.ReservationDbContext>();

        if (context.Reservations.Any())
            return;

        var screeningId1 = new Guid("11111111-1111-1111-1111-111111111111");
        var screeningId2 = new Guid("22222222-2222-2222-2222-222222222222");

        var seat1 = new SeatDetails(new Guid("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"), Row: 1, Number: 1, SeatType: "Standard");
        var seat2 = new SeatDetails(new Guid("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaab"), Row: 1, Number: 2, SeatType: "Standard");
        var seat3 = new SeatDetails(new Guid("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaac"), Row: 2, Number: 1, SeatType: "VIP");

        var (res1, tickets1) = _factory.CreateReservation(
            id: new Guid("33333333-3333-3333-3333-333333333333"),
            userId: new Guid("dddddddd-dddd-dddd-dddd-dddddddddddd"),
            screeningId: screeningId1,
            status: Domain.Enums.ReservationStatus.Confirmed,
            seats: [seat1, seat2]);

        tickets1[0].Id = Guid.Parse("55555555-5555-5555-5555-555555555555");
        tickets1[1].Id = new Guid("55555555-5555-5555-5555-555555555556");
        res1.Tickets = tickets1;
        res1.SeatLocks = CreateSeatLocks(res1, [seat1, seat2]);

        var (res2, tickets2) = _factory.CreateReservation(
            Guid.Parse("33333333-3333-3333-3333-333333333334"),
            Guid.Parse("eeeeeeee-eeee-eeee-eeee-eeeeeeeeeeee"),
            screeningId2, Domain.Enums.ReservationStatus.Locked,
            [seat3]);

        tickets2[0].Id = Guid.Parse("55555555-5555-5555-5555-555555555557");
        res2.Tickets = tickets2;
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
}
