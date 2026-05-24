namespace Reservation.API.Services.Pricing;

public interface ITicketPricingService
{
    decimal CalculateTotalPrice(int ticketCount);
    decimal CalculateTicketPrice();
}
