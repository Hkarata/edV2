namespace eDereva.Domain.Contracts.Requests;

public record SessionFilterRequest
{
    public Guid? VenueId { get; set; } = Guid.Empty;
    public Guid RegionId { get; set; } = Guid.Empty;
    public Guid DistrictId { get; set; } = Guid.Empty;
    public DateTime DateFrom { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}