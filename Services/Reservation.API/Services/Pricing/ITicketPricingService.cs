namespace Reservation.API.Services.Pricing;

public interface ITicketPricingService
{
    decimal CalculateTicketPrice(string seatType);
    decimal CalculateTotalPrice(IEnumerable<string> seatTypes);
}
