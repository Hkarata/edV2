namespace eDereva.Domain.Contracts.Requests;

public record AuthenticationRequest
{
    public string PhoneNumber { get; set; } = string.Empty;
    public string ClientHashedPassword { get; set; } = string.Empty;
}