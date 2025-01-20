using eDereva.Domain.DataProtection;

namespace eDereva.Domain.Contracts.Responses;

public record NidaUserDataResponse
{
    [SensitiveData] public string Nin { get; set; } = string.Empty;

    public string FirstName { get; set; } = string.Empty;

    public string MiddleName { get; set; } = string.Empty;

    public string LastName { get; set; } = string.Empty;

    public string Sex { get; set; } = string.Empty;

    [PiiData] public DateTime DateOfBirth { get; set; }

    public string Nationality { get; set; } = string.Empty;

    [SensitiveData] public string NationalIdNumber { get; set; } = string.Empty;
}