namespace Cinema.API.DTOs;

public record PagedResponse<T>(
    IEnumerable<T> Data,
    int Page,
    int PageSize,
    int TotalCount
);
