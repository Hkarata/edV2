using eDereva.Domain.Contracts.Requests;
using eDereva.Domain.Contracts.Responses;

namespace eDereva.Application.Repositories;

public interface IUserRepository
{
    Task<bool> CheckIfUserExists(string nin, string email, string phoneNumber, CancellationToken cancellationToken);

    Task CreateUser(CreateUserRequest request, CancellationToken cancellationToken);
    
    Task<string?> Authenticate(string phoneNumber, CancellationToken cancellationToken);
    
    Task<UserDataResponse> GetUserData(string phoneNumber, CancellationToken cancellationToken);
}