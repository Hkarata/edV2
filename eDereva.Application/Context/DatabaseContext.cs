using System.Data;
using System.Diagnostics;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace eDereva.Application.Context;

public interface IDatabaseContext : IDisposable
{
    Task<SqlDataReader> ExecuteReaderAsync(SqlCommand sqlCommand, CancellationToken cancellationToken);
    Task<int> ExecuteNonQueryAsync(SqlCommand sqlCommand, CancellationToken cancellationToken);
    Task<object> ExecuteScalarAsync(SqlCommand sqlCommand, CancellationToken cancellationToken);
}

public sealed class DatabaseContext
    (IConfiguration configuration, ILogger<DatabaseContext> logger) 
    : IDatabaseContext
{
    private readonly string _connectionString = configuration.GetConnectionString("AppDbConnection") 
                                                ?? throw new ArgumentNullException(nameof(configuration), "Connection string 'AppDbConnection' not found.");
    private SqlConnection? _connection;
    private bool _disposed;

    private async Task<SqlConnection?> GetOpenConnectionAsync(CancellationToken cancellationToken)
    {
        try
        {
            _connection ??= new SqlConnection(_connectionString);

            if (_connection.State == ConnectionState.Open) 
                return _connection;
        
            var stopwatch = Stopwatch.StartNew();
            await _connection.OpenAsync(cancellationToken);
            stopwatch.Stop();

            logger.LogInformation(
                "Database connection opened successfully. Duration: {Duration}ms",
                stopwatch.ElapsedMilliseconds);

            return _connection;
        }
        catch (Exception ex)
        {
            logger.LogError(
                ex,
                "Failed to open database connection. Error: {ErrorMessage}",
                ex.Message);
            throw;
        }
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    private void Dispose(bool disposing)
    {
        if (_disposed)
        {
            return;
        }

        if (disposing)
        {
            logger.LogInformation("Disposing database connection");
            _connection?.Dispose();
        }

        _disposed = true;
    }

    ~DatabaseContext()
    {
        Dispose(false);
    }

    public async Task<SqlDataReader> ExecuteReaderAsync(SqlCommand sqlCommand, CancellationToken cancellationToken)
    {
        var commandText = sqlCommand.CommandText;
        var commandType = sqlCommand.CommandType;
        var parameters = sqlCommand.Parameters.Cast<SqlParameter>()
            .Select(p => new { p.ParameterName, p.Value })
            .ToList();

        logger.LogInformation(
            "Executing  CommandType: {CommandType}, CommandText: {CommandText}, Parameters: {@Parameters}",
            commandType,
            commandText,
            parameters);
        
        var connection = await GetOpenConnectionAsync(cancellationToken);
        
        sqlCommand.Connection = connection;
        
        var stopwatch = Stopwatch.StartNew();
        
        try
        {
            var result = await sqlCommand.ExecuteReaderAsync(cancellationToken);
            stopwatch.Stop();

            logger.LogInformation(
                "SQL execution completed successfully. Duration: {Duration}ms",
                stopwatch.ElapsedMilliseconds);
        
            return result;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            logger.LogError(
                ex,
                "SQL execution failed after {Duration}ms. Error: {ErrorMessage}",
                stopwatch.ElapsedMilliseconds,
                ex.Message);
            throw;
        }
    }

    public async Task<int> ExecuteNonQueryAsync(SqlCommand sqlCommand, CancellationToken cancellationToken)
    {
        var commandText = sqlCommand.CommandText;
        var commandType = sqlCommand.CommandType;
        var parameters = sqlCommand.Parameters.Cast<SqlParameter>()
            .Select(p => new { p.ParameterName, p.Value })
            .ToList();

        logger.LogInformation(
            "Executing  CommandType: {CommandType}, CommandText: {CommandText}, Parameters: {@Parameters}",
            commandType,
            commandText,
            parameters);
        
        var connection = await GetOpenConnectionAsync(cancellationToken);
        
        sqlCommand.Connection = connection;
        
        var stopwatch = Stopwatch.StartNew();

        try
        {
            var result = await sqlCommand.ExecuteNonQueryAsync(cancellationToken);
            stopwatch.Stop();
        
            logger.LogInformation(
                "SQL execution completed successfully. Duration: {Duration}ms",
                stopwatch.ElapsedMilliseconds);
        
            return result;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            logger.LogError(
                ex,
                "SQL execution failed after {Duration}ms. Error: {ErrorMessage}",
                stopwatch.ElapsedMilliseconds,
                ex.Message);
            throw;
        }
    }

    public async Task<object> ExecuteScalarAsync(SqlCommand sqlCommand, CancellationToken cancellationToken)
    {
        var commandText = sqlCommand.CommandText;
        var commandType = sqlCommand.CommandType;
        var parameters = sqlCommand.Parameters.Cast<SqlParameter>()
            .Select(p => new { p.ParameterName, p.Value })
            .ToList();

        logger.LogInformation(
            "Executing  CommandType: {CommandType}, CommandText: {CommandText}, Parameters: {@Parameters}",
            commandType,
            commandText,
            parameters);
        
        var connection = await GetOpenConnectionAsync(cancellationToken);
        
        sqlCommand.Connection = connection;
        
        var stopwatch = Stopwatch.StartNew();

        try
        {
            var result = await sqlCommand.ExecuteScalarAsync(cancellationToken);
            stopwatch.Stop();
        
            logger.LogInformation(
                "SQL execution completed successfully. Duration: {Duration}ms",
                stopwatch.ElapsedMilliseconds);
        
            return result;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            logger.LogError(
                ex,
                "SQL execution failed after {Duration}ms. Error: {ErrorMessage}",
                stopwatch.ElapsedMilliseconds,
                ex.Message);
            throw;
        }
    }
}