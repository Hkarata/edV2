using eDereva.Application.Context;
using eDereva.Application.Repositories;
using eDereva.Application.Services;
using eDereva.Domain.Contracts.Requests;
using eDereva.Domain.Contracts.Responses;
using eDereva.Domain.SQL_Queries;
using Microsoft.Data.SqlClient;

namespace eDereva.Infrastructure.Repository;

public class UserRepository(IDatabaseContext context, IPasswordService passwordService)
    : IUserRepository
{
    public async Task<bool> CheckIfUserExists(string nin, string email, string phoneNumber,
        CancellationToken cancellationToken)
    {
        await using var sqlCommand = new SqlCommand(UserQueries.CheckIfUserExists);
        sqlCommand.Parameters.AddWithValue("@NationalID", nin);
        sqlCommand.Parameters.AddWithValue("@Email", email);
        sqlCommand.Parameters.AddWithValue("@PhoneNumber", phoneNumber);

        var result = await context.ExecuteScalarAsync(sqlCommand, cancellationToken);

        return result != null;
    }

    public async Task CreateUser(CreateUserRequest request, CancellationToken cancellationToken)
    {
        await using var sqlCommand = new SqlCommand(UserQueries.InsertUser);
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

    public async Task<string?> Authenticate(string phoneNumber, CancellationToken cancellationToken)
    {
        await using var sqlCommand = new SqlCommand(UserQueries.AuthenticateUser);
        sqlCommand.Parameters.AddWithValue("@PhoneNumber", phoneNumber);
        
        var hashedPassword = await context.ExecuteScalarAsync(sqlCommand, cancellationToken);
        
        return hashedPassword?.ToString();
    }

    public async Task<UserDataResponse> GetUserData(string phoneNumber, CancellationToken cancellationToken)
    {
        await using var sqlCommand = new SqlCommand(UserQueries.GetUserDetails);
        sqlCommand.Parameters.AddWithValue("@PhoneNumber", phoneNumber);

        await using var sqlDataReader = await context.ExecuteReaderAsync(sqlCommand, cancellationToken);
        
        var response = new UserDataResponse();

        if (!sqlDataReader.HasRows)
        {
            return response;
        }

        if (!await sqlDataReader.ReadAsync(cancellationToken)) return response;
        
        response.UserId = sqlDataReader.GetGuid(sqlDataReader.GetOrdinal("UserId"));
        response.Nin = sqlDataReader.GetString(sqlDataReader.GetOrdinal("NationalID"));
        response.Email = sqlDataReader.GetString(sqlDataReader.GetOrdinal("Email"));
        response.PhoneNumber = sqlDataReader.GetString(sqlDataReader.GetOrdinal("PhoneNumber"));
        response.GivenName = sqlDataReader.GetString(sqlDataReader.GetOrdinal("GivenName"));
        response.Surname = sqlDataReader.GetString(sqlDataReader.GetOrdinal("Surname"));

        return response;
    }
}