namespace Reservation.API.ExternalServices;

public record ScreeningDetails(Guid Id, Guid MovieId, Guid HallId, Guid CinemaId, DateTime StartTime, string Format, string Status);
