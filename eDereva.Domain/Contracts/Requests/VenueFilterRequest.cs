namespace eDereva.Domain.Contracts.Requests;

public record VenueFilterRequest
{
    public Guid RegionId { get; init; }
    public Guid? DistrictId { get; init; }
    public string? SearchTerm { get; init; }
    public int? MinCapacity { get; init; }
    public int? MaxCapacity { get; init; }
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 20;
}