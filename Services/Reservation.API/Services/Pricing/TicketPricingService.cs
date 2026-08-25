namespace Reservation.API.Services.Pricing;

public class TicketPricingService : ITicketPricingService
{
    private const decimal StandardPrice = 10.0m;
    private const decimal VipPrice = 15.0m;
    private const decimal CouplePrice = 18.0m;
    private const decimal AccessiblePrice = 10.0m;

    public decimal CalculateTicketPrice(string seatType) => seatType switch
    {
        "VIP" => VipPrice,
        "Couple" => CouplePrice,
        "Accessible" => AccessiblePrice,
        _ => StandardPrice
    };

    public decimal CalculateTotalPrice(IEnumerable<string> seatTypes) => seatTypes.Sum(CalculateTicketPrice);
}
