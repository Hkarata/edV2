using eDereva.Domain.Contracts.Requests;

namespace eDereva.Application.Repositories;

public interface IUserRepository
{
    Task<bool> CheckIfUserExists(string nin, string email, string phoneNumber, CancellationToken cancellationToken);

    Task CreateUser(CreateUserRequest request, CancellationToken cancellationToken);
}