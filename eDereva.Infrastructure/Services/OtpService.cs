using System.Text;
using eDereva.Application.Context;
using eDereva.Application.Services;
using eDereva.Domain.SQL_Queries;
using Microsoft.Data.SqlClient;

namespace eDereva.Infrastructure.Services;

public class OtpService(IDatabaseContext context) : IOtpService
{
    private static readonly Random Random = new();

    public async Task<string> GenerateOtpAsync(string phoneNumber, CancellationToken ct)
    {
        string otp;
        bool isUnique;

        // Ensure uniqueness by checking the OTP asynchronously
        do
        {
            otp = GenerateOtp();
            isUnique = await IsOtpUniqueAsync(otp, ct);
        } while (!isUnique);

        await using var sqlCommand = new SqlCommand(OtpQueries.CheckIfOtpCodeExists);
        sqlCommand.Parameters.AddWithValue("@PhoneNumber", phoneNumber);
        sqlCommand.Parameters.AddWithValue("@OtpCode", otp);

        await context.ExecuteNonQueryAsync(sqlCommand, ct);

        return otp;
    }

    public async Task<bool> ConfirmOtp(string phoneNumber, string otp, CancellationToken ct)
    {
        await using var sqlCommand = new SqlCommand(OtpQueries.ConfirmOtpCode);
        sqlCommand.Parameters.AddWithValue("@PhoneNumber", phoneNumber);
        sqlCommand.Parameters.AddWithValue("@OtpCode", otp);

        var result = (bool)await context.ExecuteScalarAsync(sqlCommand, ct);

        return result;
    }

    private static string GenerateOtp()
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        var otp = new StringBuilder(6);

        for (var i = 0; i < 6; i++) otp.Append(chars[Random.Next(chars.Length)]);

        return otp.ToString();
    }

    private async Task<bool> IsOtpUniqueAsync(string otp, CancellationToken ct)
    {
        await using var sqlCommand = new SqlCommand(OtpQueries.CheckIfOtpCodeExists);
        sqlCommand.Parameters.AddWithValue("@OtpCode", otp);

        var result = (bool)await context.ExecuteScalarAsync(sqlCommand, ct);

        return result;
    }
}