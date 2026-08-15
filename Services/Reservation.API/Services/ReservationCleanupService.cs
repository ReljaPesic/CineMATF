using Reservation.API.Repositories;

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
            var service = scope.ServiceProvider.GetRequiredService<IReservationService>();

            await repository.CleanExpiredLocksAsync();

            var expiredReservations = await repository.GetExpiredReservationsAsync();
            foreach (var reservation in expiredReservations)
            {
                await service.ExpireReservationAsync(reservation.Id);
            }

            await Task.Delay(_cleanupInterval, stoppingToken);
        }
    }
}
