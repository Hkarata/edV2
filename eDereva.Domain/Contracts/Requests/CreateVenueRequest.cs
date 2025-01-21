namespace eDereva.Domain.Contracts.Requests;

public class CreateVenueRequest
{
    public string Name { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public List<string>? ImageUrls { get; set; }
    public int Capacity { get; set; }
    public Guid DistrictId { get; set; }
}