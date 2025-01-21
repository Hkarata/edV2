using eDereva.Domain.Contracts.Requests;
using eDereva.Domain.Contracts.Responses;
using Microsoft.Extensions.Logging;

namespace eDereva.Domain.Loggers;

public static partial class UserLogger
{
    [LoggerMessage(LogLevel.Information, "User data successfully retrieved from NIDA")]
    public static partial void LogUserDataRetrievedFromNida(
        this ILogger logger,
        [LogProperties] NidaUserDataResponse nidaUserDataResponse
    );

    [LoggerMessage(LogLevel.Information, "Checking if user exists")]
    public static partial void LogCheckingIfUserExists(
        this ILogger logger,
        [LogProperties] UserCheckRequest userCheckRequest
    );

    [LoggerMessage(LogLevel.Information, "User created successfully")]
    public static partial void LogUserCreated(
        this ILogger logger,
        [LogProperties] CreateUserRequest request
    );
}