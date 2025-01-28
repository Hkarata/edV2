namespace eDereva.Domain.Contracts.Responses;

public class SessionResponse
{
    public Guid Id { get; set; }
    public DateTime Date { get; set; }
    public string Status { get; set; } = string.Empty;
    public TimeSpan StartTime { get; set; }
    public TimeSpan EndTime { get; set; }
    public int AvailableSeats { get; set; }
    public Guid? ContingencyId { get; set; }
    public string VenueName { get; set; } = string.Empty;
    public string VenueAddress { get; set; } = string.Empty;
    public string? ContingencyType { get; set; } = string.Empty;
    public string? ContingencyExplanation { get; set; } = string.Empty;
    public string DistrictName { get; set; } = string.Empty;
    public string RegionName { get; set; } = string.Empty;
}