using Entities = Reservation.API.Domain.Entities;

namespace Reservation.API.Services;

public class DataSeeder(IServiceProvider serviceProvider) : IHostedService
{
    private readonly IServiceProvider _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));

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

        var reservations = new[]
        {
            new Entities.Reservation
            {
                Id = new Guid("33333333-3333-3333-3333-333333333333"),
                UserId = new Guid("dddddddd-dddd-dddd-dddd-dddddddddddd"),
                ScreeningId = screeningId1,
                Status = Domain.Enums.ReservationStatus.Confirmed,
                TotalPrice = 20.0m,
                CreatedAt = DateTime.UtcNow.AddDays(-2),
                ExpiresAt = DateTime.UtcNow.AddDays(-1),
                Tickets =
                {
                    new Entities.Ticket
                    {
                        Id = Guid.Parse("55555555-5555-5555-5555-555555555555"),
                        SeatId = seatId1,
                        SeatRow = 1,
                        SeatNumber = 1,
                        Price = 10.0m,
                        QrCode = Guid.NewGuid().ToString()
                    },
                    new Entities.Ticket
                    {
                        Id = new Guid("55555555-5555-5555-5555-555555555556"),
                        SeatId = seatId2,
                        SeatRow = 1,
                        SeatNumber = 2,
                        Price = 10.0m,
                        QrCode = Guid.NewGuid().ToString()
                    }
                }
            },
            new Entities.Reservation
            {
                Id = Guid.Parse("33333333-3333-3333-3333-333333333334"),
                UserId = Guid.Parse("eeeeeeee-eeee-eeee-eeee-eeeeeeeeeeee"),
                ScreeningId = screeningId2,
                Status = Domain.Enums.ReservationStatus.Locked,
                TotalPrice = 10.0m,
                CreatedAt = DateTime.UtcNow.AddHours(-1),
                ExpiresAt = DateTime.UtcNow.AddMinutes(30),
                Tickets =
                {
                    new Entities.Ticket
                    {
                        Id = Guid.Parse("55555555-5555-5555-5555-555555555557"),
                        SeatId = seatId3,
                        SeatRow = 2,
                        SeatNumber = 1,
                        Price = 10.0m,
                        QrCode = Guid.NewGuid().ToString()
                    }
                }
            }
        };

        await context.Reservations.AddRangeAsync(reservations, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}
