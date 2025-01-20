using eDereva.Domain.DataProtection;

namespace eDereva.Domain.Contracts.Requests;

public record UserCheckRequest
{
    [PiiData] public string Nin { get; set; } = string.Empty;

    [SensitiveData] public string Email { get; set; } = string.Empty;

    [PiiData] public string Phone { get; set; } = string.Empty;
}