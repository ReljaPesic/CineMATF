namespace Cinema.API.DTOs;

public record CreateHallsRequest(IEnumerable<HallRequest> Halls);
