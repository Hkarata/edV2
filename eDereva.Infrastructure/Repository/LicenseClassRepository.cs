using eDereva.Application.Context;
using eDereva.Application.Repositories;
using eDereva.Domain.Contracts.Responses;
using eDereva.Domain.SQL_Queries;
using Microsoft.Data.SqlClient;

namespace eDereva.Infrastructure.Repository;

public class LicenseClassRepository (IDatabaseContext context) : ILicenseClassRepository
{
    public async Task BulkInsertUserLicensesAsync(Guid userId, List<short> licenseClassIds, CancellationToken cancellationToken)
    {
        // Build the SQL dynamically based on number of licenses
        var valuePlaceholders = string.Join(",\n", 
            licenseClassIds.Select((_, index) => $"(@UserID, @LicenseClassID{index})"));
        
        var sql = $"""
                   
                           INSERT INTO [Identity].UserLicenseClasses (UserID, LicenseClassID)
                           VALUES {valuePlaceholders}
                   """;

        var sqlCommand = new SqlCommand(sql);
        
        // Add UserID parameter once since it's the same for all records
        sqlCommand.Parameters.AddWithValue("@UserID", userId);
    
        // Add parameters for each license class ID
        for (var i = 0; i < licenseClassIds.Count; i++)
        {
            sqlCommand.Parameters.AddWithValue($"@LicenseClassID{i}", licenseClassIds[i]);
        }
        
        await context.ExecuteScalarAsync(sqlCommand, cancellationToken);
    }

    public async Task<List<LicenseClassInfo>> GetUserLicensesAsync(Guid userId, CancellationToken cancellationToken)
    {
        var licenses = new List<LicenseClassInfo>();

        // Prepare the SQL command
        var sqlCommand = new SqlCommand(LicenseQueries.GetUserLicenseClasses);
        sqlCommand.Parameters.AddWithValue("@UserID", userId);

        // Execute the reader
        var sqlDataReader = await context.ExecuteReaderAsync(sqlCommand, cancellationToken);

        if (!sqlDataReader.HasRows)
        {
            return licenses; // Return an empty list if no rows are found
        }
        
        // Iterate through the results
        while (await sqlDataReader.ReadAsync(cancellationToken))
        {
            licenses.Add(new LicenseClassInfo
            {
                LicenseClassId = sqlDataReader.GetInt32(sqlDataReader.GetOrdinal("LicenseClassID")),
                ClassName = sqlDataReader.GetString(sqlDataReader.GetOrdinal("ClassName")),
                Description = sqlDataReader.IsDBNull(sqlDataReader.GetOrdinal("Description"))
                    ? null
                    : sqlDataReader.GetString(sqlDataReader.GetOrdinal("Description"))
            });
        }

        return licenses;
    }

    public async Task AddLicenseClassAsync(Guid userId, int licenseClassId, CancellationToken cancellationToken)
    {
        var sqlCommand = new SqlCommand(LicenseQueries.AddSingleLicenseClass);
        sqlCommand.Parameters.AddWithValue("@UserID", userId);
        sqlCommand.Parameters.AddWithValue("@LicenseClassID", licenseClassId);
        
        await context.ExecuteNonQueryAsync(sqlCommand, cancellationToken);
    }

    public async Task RemoveLicenseClassAsync(Guid userId, int licenseClassId, CancellationToken cancellationToken)
    {
        var sqlCommand = new SqlCommand(LicenseQueries.DeleteUserLicenseClass);
        sqlCommand.Parameters.AddWithValue("@UserID", userId);
        sqlCommand.Parameters.AddWithValue("@LicenseClassID", licenseClassId);
        
        await context.ExecuteNonQueryAsync(sqlCommand, cancellationToken);
    }
}