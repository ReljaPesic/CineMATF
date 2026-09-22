namespace Reservation.API.Settings;

public class ReservationOptions
{
    public int LockDurationMinutes { get; set; } = 10;

    // Extends a reservation's hold (and its seat locks) once Stripe Checkout starts, so a
    // slow checkout doesn't lose the race against the base lock timeout. Stripe requires a
    // Checkout Session's expires_at to be at least 30 minutes out, so this must stay >= 30.
    public int CheckoutHoldExtensionMinutes { get; set; } = 30;
}
