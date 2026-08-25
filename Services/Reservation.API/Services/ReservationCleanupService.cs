using Reservation.API.Repositories;

namespace Reservation.API.Services;

public class ReservationCleanupService(
    IServiceProvider serviceProvider,
    ILogger<ReservationCleanupService> logger) : BackgroundService
{
    private readonly IServiceProvider _serviceProvider = serviceProvider;
    private readonly ILogger<ReservationCleanupService> _logger = logger;
    private readonly TimeSpan _cleanupInterval = TimeSpan.FromMinutes(1);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var repository = scope.ServiceProvider.GetRequiredService<IReservationRepository>();
                var service = scope.ServiceProvider.GetRequiredService<IReservationService>();

                var expiredReservations = await repository.GetExpiredReservationsAsync();
                foreach (var reservation in expiredReservations)
                {
                    await service.ExpireReservationAsync(reservation.Id);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Reservation cleanup cycle failed");
            }

            await Task.Delay(_cleanupInterval, stoppingToken);
        }
    }
}
