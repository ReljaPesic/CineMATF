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

        var seatId1 = new Guid("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");
        var seatId2 = new Guid("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaab");
        var seatId3 = new Guid("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaac");

        var (res1, tickets1) = _factory.CreateReservation(
            id: new Guid("33333333-3333-3333-3333-333333333333"),
            userId: new Guid("dddddddd-dddd-dddd-dddd-dddddddddddd"),
            screeningId: screeningId1,
            status: Domain.Enums.ReservationStatus.Confirmed,
            seatIds: [seatId1, seatId2]);

        tickets1[0].Id = Guid.Parse("55555555-5555-5555-5555-555555555555");
        tickets1[0].SeatRow = 1;
        tickets1[0].SeatNumber = 1;
        tickets1[1].Id = new Guid("55555555-5555-5555-5555-555555555556");
        tickets1[1].SeatRow = 1;
        tickets1[1].SeatNumber = 2;
        res1.Tickets = tickets1;

        var (res2, tickets2) = _factory.CreateReservation(
            Guid.Parse("33333333-3333-3333-3333-333333333334"),
            Guid.Parse("eeeeeeee-eeee-eeee-eeee-eeeeeeeeeeee"),
            screeningId2, Domain.Enums.ReservationStatus.Locked,
            [seatId3]);

        tickets2[0].Id = Guid.Parse("55555555-5555-5555-5555-555555555557");
        tickets2[0].SeatRow = 2;
        tickets2[0].SeatNumber = 1;
        res2.Tickets = tickets2;

        var reservations = new[] { res1, res2 };

        await context.Reservations.AddRangeAsync(reservations, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}
