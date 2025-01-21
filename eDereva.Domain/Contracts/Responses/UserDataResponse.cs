namespace eDereva.Domain.Contracts.Responses;

public record UserDataResponse
{
    public Guid UserId { get; set; }
    public string Nin { get; set; } = string.Empty;
    public string GivenName { get; set; } = string.Empty;
    public string Surname { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
}
