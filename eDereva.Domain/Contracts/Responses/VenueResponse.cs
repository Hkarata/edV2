namespace eDereva.Domain.Contracts.Responses;

public record VenueResponse
{
    public Guid VenueId { get; set; }
    public string VenueName { get; set; } = string.Empty;
    public string VenueAddress { get; set; } = string.Empty;
    public List<string>? Images { get; set; }
    public int Capacity { get; set; }
    public Guid DistrictId { get; set; }
    public string DistrictName { get; set; } = string.Empty;
    public Guid RegionId { get; set; }
    public string RegionName { get; set; } = string.Empty;
}