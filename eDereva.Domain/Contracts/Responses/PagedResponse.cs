namespace eDereva.Domain.Contracts.Responses;

public record PagedResponse<T>
{
    public List<T> Items { get; init; } = [];
    public int TotalCount { get; init; }
    public int TotalPages { get; init; }
    public int CurrentPage { get; init; }
    public bool HasNextPage { get; init; }
    public bool HasPreviousPage { get; init; }
}