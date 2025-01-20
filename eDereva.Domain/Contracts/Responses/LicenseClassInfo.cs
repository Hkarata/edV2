namespace eDereva.Domain.Contracts.Responses;

public class LicenseClassInfo
{
    public int LicenseClassId { get; set; }
    public string ClassName { get; set; } = null!;
    public string? Description { get; set; }
}