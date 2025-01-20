using eDereva.Application.Context;
using eDereva.Application.Repositories;
using eDereva.Application.Services;
using eDereva.Domain.Contracts.Requests;
using eDereva.Domain.SQL_Queries;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;

namespace eDereva.Infrastructure.Repository;

public class UserRepository(IDatabaseContext context, IPasswordService passwordService) 
    : IUserRepository
{
    public async Task<bool> CheckIfUserExists(string nin, string email, string phoneNumber, CancellationToken cancellationToken)
    {
        var sqlCommand = new SqlCommand(UserQueries.CheckIfUserExists);
        sqlCommand.Parameters.AddWithValue("@NationalID", nin);
        sqlCommand.Parameters.AddWithValue("@Email", email);
        sqlCommand.Parameters.AddWithValue("@PhoneNumber", phoneNumber);
        
        var result = await context.ExecuteScalarAsync(sqlCommand, cancellationToken);
        
        return result != null;
    }

    public async Task CreateUser(CreateUserRequest request, CancellationToken cancellationToken)
    {
        var sqlCommand = new SqlCommand(UserQueries.InsertUser);
        sqlCommand.Parameters.AddWithValue("@NationalID", request.Nin);
        sqlCommand.Parameters.AddWithValue("@NationalIDType", request.NationalIDType.ToUpper());
        sqlCommand.Parameters.AddWithValue("@Email", request.Email);
        sqlCommand.Parameters.AddWithValue("@PhoneNumber", request.PhoneNumber);
        sqlCommand.Parameters.AddWithValue("@PasswordHash", passwordService.HashPassword(request.Password));
        sqlCommand.Parameters.AddWithValue("@FirstName", request.FirstName);
        sqlCommand.Parameters.AddWithValue("@SurName", request.LastName);
        sqlCommand.Parameters.AddWithValue("@MiddleName", request.MiddleName);
        sqlCommand.Parameters.AddWithValue("@Sex", request.Sex);
        sqlCommand.Parameters.AddWithValue("@DateOfBirth", request.DateOfBirth);
        
        await context.ExecuteScalarAsync(sqlCommand, cancellationToken);
    }
}