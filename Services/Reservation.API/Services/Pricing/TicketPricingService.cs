namespace Reservation.API.Services.Pricing;

public class TicketPricingService : ITicketPricingService
{
    private const decimal BasePrice = 10.0m;

    public decimal CalculateTotalPrice(int ticketCount) => ticketCount * BasePrice;
    public decimal CalculateTicketPrice() => BasePrice;
}
