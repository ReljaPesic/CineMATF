namespace Reservation.API.Settings;

public class StripeSettings
{
    public string SecretKey { get; set; } = string.Empty;
    public string WebhookSecret { get; set; } = string.Empty;
    public string Currency { get; set; } = "eur";
    public string SuccessUrlBase { get; set; } = string.Empty;
    public string CancelUrlBase { get; set; } = string.Empty;
}
