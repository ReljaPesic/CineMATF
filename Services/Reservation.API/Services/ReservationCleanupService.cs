using Reservation.API.Data;
using Reservation.API.Domain.Enums;
using Reservation.API.Repositories;
using Microsoft.Extensions.DependencyInjection;

namespace Reservation.API.Services;

public class ReservationCleanupService(IServiceProvider serviceProvider) : BackgroundService
{
    private readonly IServiceProvider _serviceProvider = serviceProvider;
    private readonly TimeSpan _cleanupInterval = TimeSpan.FromMinutes(1);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            using var scope = _serviceProvider.CreateScope();
            var repository = scope.ServiceProvider.GetRequiredService<IReservationRepository>();
            await repository.CleanExpiredLocksAsync();

            var expiredReservations = await repository.GetExpiredReservationsAsync();
            foreach (var reservation in expiredReservations)
            {
                await repository.UpdateReservationStatusAsync(reservation.Id, ReservationStatus.Expired);
                foreach (var seatLock in reservation.SeatLocks)
                {
                    await repository.DeleteSeatLockAsync(seatLock.Id);
                }
            }

            await Task.Delay(_cleanupInterval, stoppingToken);
        }
    }
}
