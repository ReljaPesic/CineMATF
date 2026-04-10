namespace Cinema.API.DTOs;

public record CreateHallsResponse(
    int Created,
    IEnumerable<FailedHall> Failed
);

public record FailedHall(
    string Name,
    string Error
);