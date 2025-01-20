using eDereva.Domain.DataProtection;

namespace eDereva.Domain.Contracts.Requests;

public record CreateUserRequest
{
    [PiiData]
    public string Nin { get; set; } = string.Empty;
    
    public string NationalIDType { get; set; } = string.Empty;
    
    public string FirstName { get; set; } = string.Empty;
    
    public string MiddleName { get; set; } = string.Empty;
    
    public string LastName { get; set; } = string.Empty;
    
    public string Sex { get; set; } = string.Empty;
    
    [SensitiveData]
    public DateTime DateOfBirth { get; set; }
    
    [PiiData]
    public string PhoneNumber { get; set; } = string.Empty;
    
    public string Email { get; set; } = string.Empty;
    
    [SensitiveData]
    public string Password { get; set; } = string.Empty;
}